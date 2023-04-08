using System;
using System.IO.Ports;
using System.Threading;
using NLog;
using NLog.Config;
using NLog.Targets;
using System.Diagnostics;
using System.Collections.Generic;
namespace ServoControl;

public delegate void ProcessReceivedMessageCallBack();



public class ServoController
{

	enum ReadState {
		ClientID,
		Data,
		Checksum,
		Success,
		Decoded,
		Failure
	}



	private SerialPort serialPort;
	private Thread statusMonitor;
	private int commandCounter = 0;
	private HashSet<string> executingCommands = new HashSet<string>();
	private string commandId = "";
	private ReadState readerState = ReadState.ClientID;
	private byte readerChecksum = 0;
	private byte[] readerData = new byte[255];
	// the amount of currently read data bytes in the current message
	private int readerDataPosition = 0;

	// the number of data bytes are to be read in the next received message
	private int waitForDataResult = 0;
	private ProcessReceivedMessageCallBack? processMessageCallback = null;
	private bool stopMonitor = false;

	public byte clientAddress { get; set; } = 0xE0;
	public int errorCount { get; private set;} = 0;
	
	// these values are updated automatically by a thread
	public int encoderRead { get; private set;} = 0;
	public int encoderPosition { get; private set; } =0;
	public int absEncoderPosition { get; private set; } =0;
	public float estimatedEncoderSpeed { get; private set; } =0;
	public int encoderRequestInterval { get; set; } = 100;
	private Stopwatch encoderReadTimer = new Stopwatch();
	private long encoderRequestTime = 0;
	private long previousEncoderReadTime = 0;
	
	// these values are only updated after calling a method
	public float angleError {get; private set;} = 0.0f;
	public int shaftStatus {get; private set;} = 0;

	public bool powerEnabled {get; private set;} = false;

	public bool constantSpeedEnabled {get; private set;} = false;
	public bool constantSpeedGuard {get; private set;} = false;
	public bool stallDetected {get; private set;} = false;
	private int constantSpeedSetpoint = 0;

	public int debug { get; set; } = 0;

	private LoggingConfiguration logConfig = new NLog.Config.LoggingConfiguration();
	private NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();
	
	public ServoController() {
		serialPort = new SerialPort();	
		statusMonitor = new Thread(new ThreadStart(Monitor));
			    
		// Apply config           
		NLog.LogManager.Configuration = logConfig;
		encoderReadTimer.Start();
	}
	

	public void Open() {
		if (debug>=1) {
			var logfile = new NLog.Targets.FileTarget("logfile") { FileName = "log.txt" };
			logConfig.AddRule(LogLevel.Debug, LogLevel.Fatal, logfile);
		}
		if (debug>=2) {
			var logconsole = new NLog.Targets.ConsoleTarget("logconsole");
			logConfig.AddRule(LogLevel.Debug, LogLevel.Fatal, logconsole);
		}
		NLog.LogManager.Configuration = logConfig;
		Logger.Info($"Opening serial port {serialPort.PortName}");
			
		serialPort.PortName = "/dev/ttyUSB0";
		serialPort.BaudRate = 38400;
 		serialPort.DataReceived += new SerialDataReceivedEventHandler(DataReceivedHandler);
		serialPort.ReadTimeout = 0;
		serialPort.Open();	
		statusMonitor.Start();
	}	

	public void Close() {
		lock (this) {
			serialPort.Close();
			stopMonitor = true;
		}
		
	}

	private (int, bool, bool) processReceivedData(byte[] in_buffer, int expectedLength) {
		int i = 0;
		while (i<in_buffer.Length) {
			if (readerState == ReadState.ClientID) {
				if (in_buffer[i] == clientAddress) {
					Logger.Debug($"{commandId}: Received client id");
					readerChecksum = clientAddress;
					readerDataPosition=0;
					readerState = ReadState.Data;
				}
				i+=1;
			} else if (readerState == ReadState.Data) {
				if (readerDataPosition<expectedLength) {
					Logger.Debug($"{commandId}: Received data {readerDataPosition}/{expectedLength}");
					readerData[readerDataPosition] = in_buffer[i];
					readerChecksum += in_buffer[i];
					readerDataPosition++;
					i+=1;
				} else {
					readerState = ReadState.Checksum;
					if (readerChecksum == in_buffer[i]) {
						Logger.Debug($"{commandId}: Checksum ok");
						readerState = ReadState.Success;
						i+=1;
						return (i, true, false);
					} else {
						Logger.Debug($"{commandId}: Checksum err");
						errorCount+=1;
						readerState = ReadState.Failure;
						// do not advance buffer position
						return (i, false, true);
					}
				}
			}
		}
		return (i, false, false);
	}

	private void DataReceivedHandler(
			object sender,
			SerialDataReceivedEventArgs e)
    {
		SerialPort sp = (SerialPort)sender;
		int avail = sp.BytesToRead;
		Logger.Debug($"{commandId}: Data Received {avail}");
		
		byte[] in_buffer = new byte[avail];
		int actual = sp.Read(in_buffer,0, avail);
		Logger.Debug($"{commandId}: Data Read :{actual}");
		
		if (waitForDataResult!=0) {
			(int pos, bool success, bool failure) = processReceivedData(in_buffer, waitForDataResult);
			if (success) {
				if (processMessageCallback != null) {
					Logger.Debug($"{commandId}: run callback");
					processMessageCallback();
				}
			} else if (failure) {
				Logger.Error($"{commandId}: Failed to read data message");
				EndCommand(commandId);
			} else {
				
			}
		}
	}

	private void processEncoderMessage() {
		Logger.Debug($"{commandId} Process enc message");
		string myCommandId = commandId;
		if (readerDataPosition==2) {
			readerState = ReadState.Decoded;
			int newEncoderPosition = (readerData[0]<<8) + readerData[1];
		
			long dt = encoderRequestTime - previousEncoderReadTime;

			int dx1 = newEncoderPosition - encoderPosition;
			int dx2 = newEncoderPosition + 65536 - encoderPosition;
			int dx3 = newEncoderPosition - 65536 - encoderPosition;
			int dx = 0;
			if (Math.Abs(dx1) < Math.Abs(dx2)) {
				dx = dx1;	
			} else {
				dx = dx2;
			}
			if (Math.Abs(dx3) < Math.Abs(dx)) {
				dx = dx3;
			}
		


			absEncoderPosition+=dx;
			estimatedEncoderSpeed = (float)dx/(((float)dt)/1000000.0f);
			//Console.WriteLine($"dx1={dx1} dx2={dx2} dx3={dx3} dx={dx} dt={dt}");
			previousEncoderReadTime = encoderRequestTime;	
			encoderPosition = newEncoderPosition;

			Logger.Debug($"Guard? {constantSpeedEnabled} and {constantSpeedGuard}");
			if (constantSpeedEnabled && constantSpeedGuard) {
				float expectedEncoderSpeed = constantSpeedSetpoint*700;
				float speedDiff = estimatedEncoderSpeed - expectedEncoderSpeed; 
				Logger.Debug($"Guard encoder speed, expected {expectedEncoderSpeed} diff={speedDiff}");
				if (Math.Abs(estimatedEncoderSpeed)<(Math.Abs(expectedEncoderSpeed)*0.25)) {
					Logger.Debug($"Guard encoder stall detected");
					stallDetected = true;
				}
			}

			Logger.Debug($"{myCommandId} Process encoder message: pos={encoderPosition} sp={estimatedEncoderSpeed}");
			encoderRead+=1;
			EndCommand(myCommandId);
		} else {
			readerState = ReadState.Failure;
			errorCount+=1;
			Logger.Debug($"Process encoder message - incorrect length");
			EndCommand(myCommandId);
		}
	}

	private void processIdleMessage() {
		readerState = ReadState.Decoded;
		EndCommand(commandId);
	}

	private void processAngleErrorMessage() {
		Logger.Debug($"Process angle error message");
		if (readerDataPosition==2) {
			readerState = ReadState.Decoded;
			Logger.Debug($"Process angle error message: {readerData[0]},{readerData[1]}");
			int angleError_int = (readerData[0]<<8) + readerData[1];
			angleError = (angleError_int/65536)*360.0f;
		} else {
			readerState = ReadState.Failure;
			Logger.Debug($"Process angle error message - incorrect length");
		}
		EndCommand(commandId);
	}

	private void processMotorShaftStatusMessage() {
		Logger.Debug($"Process shaft status message");
		if (readerDataPosition==1) {
			readerState = ReadState.Decoded;
			Logger.Debug($"Process shaft status message: {readerData[0]}");
			shaftStatus = readerData[0];
		} else {
			readerState = ReadState.Failure;
			Logger.Debug($"Process shaft status message - incorrect length");
		}
		EndCommand(commandId);
	}

	private void processPowerEnableStatusMessage() {
		Logger.Debug($"Process power enable status message");
		if (readerDataPosition==1) {
			readerState = ReadState.Decoded;
			Logger.Debug($"Process power enable status message: {readerData[0]}");
			if (readerData[0]==1) {
				powerEnabled = true;
			} else if (readerData[0]==2) {
				powerEnabled = false;
			} else {
				readerState = ReadState.Failure;
				Logger.Debug($"Process power enable status message - error result");
			}
		} else {
			readerState = ReadState.Failure;
			Logger.Debug($"Process power enable status message - incorrect length");
		}
		EndCommand(commandId);
	}


	private void processStatusMessage() {
		Logger.Debug($"Process status message");
			
		if (readerDataPosition==1) {
			if (readerData[0] == 1) {
				readerState = ReadState.Decoded;
				Logger.Debug($"Process status message - success");
			} else {
				readerState = ReadState.Failure;
				errorCount+=1;
				Logger.Debug($"Process status - failure");
			}
		} else {
			readerState = ReadState.Failure;
			errorCount+=1;
			Logger.Debug($"Process status message - incorrect length");
		}
		EndCommand(commandId);
	}

	private string StartCommand(string cmdName, int expectedLength, ProcessReceivedMessageCallBack? processCallback) {
		commandId = cmdName+(commandCounter++);
		executingCommands.Add(commandId);
		readerState = ReadState.ClientID;
		waitForDataResult = expectedLength;
		processMessageCallback = processCallback; 
		string cmds = string.Join(",", executingCommands);
		Logger.Debug($"Start {commandId} command (other {cmds})");
		return commandId;
	}

	private void EndCommand(string cmdId) {
		Logger.Debug($"End {commandId} command (other {cmds})");
		waitForDataResult = 0;
		commandId = "";		
		executingCommands.Remove(cmdId);
		string cmds = string.Join(",", executingCommands);
	}

	public bool StopMotor(bool doLock = true) {
		byte[] message = new byte[3];
		message[0] = clientAddress;
		message[1] = (byte)0xF7;
		SetChecksum(message);

		bool _runcmd() {
			string cmdid = StartCommand("StopMotor", 1, processStatusMessage);
			constantSpeedEnabled = false;
			serialPort.Write(message,0, message.Length);
			return WaitForResult(cmdid);
		}

		if (doLock) {
			lock(this) {
				return _runcmd();
			}
		} else {
			return _runcmd();
		}
	}

	public bool GetPowerEnable(out bool ena, bool doLock = true) {
		byte[] message = new byte[3];
		message[0] = clientAddress;
		message[1] = (byte)0x3a;
		SetChecksum(message);
		bool result = false;
		
		void _runcmd() {
			string cmdid = StartCommand("GetPowerEnable", 1, processPowerEnableStatusMessage);
			serialPort.Write(message,0, message.Length);		
			result =  WaitForResult(cmdid);
		}
		
		if (doLock) {
			lock(this) {
				_runcmd();
			}
		} else {
			_runcmd();
		}
		ena = powerEnabled;
		return result;		
	}

	public bool PowerEnable(bool ena, bool doLock = true) {
		byte[] message = new byte[4];
		message[0] = clientAddress;
		message[1] = (byte)0xF3;
		message[2] = ena ? (byte)1 : (byte)0;
		SetChecksum(message);
		//Logger.Debug("Prepared PowerEnable message, acquire lock");
		
		bool _runcmd() {
			//Logger.Debug("Prepared PowerEnable message, acquire lock done");
			string cmdid = StartCommand("PowerEnable", 1, processStatusMessage);
			constantSpeedEnabled = false;
			serialPort.Write(message,0, message.Length);		
			bool result = WaitForResult(cmdid);
			if (result) {
				powerEnabled = ena;
			}
			return result;
		}
			
		if (doLock) {
			lock(this) {
				return _runcmd();
			}	
		} else {
			return _runcmd();
		}
	}

	/*
	 * This also disables stall protection!
	 * Doesn't work as advertised
	 */
	public bool ReleaseHWProtection() {
		byte[] message = new byte[3];
		message[0] = clientAddress;
		message[1] = (byte)0x3D;
		SetChecksum(message);
		lock(this) {
			string cmdid = StartCommand("ReleaseProtection", 1, processStatusMessage);
			serialPort.Write(message,0, message.Length);		
			return WaitForResult(cmdid);
		}	
		
	}

	public void ReleaseSWProtection() {
		stallDetected = false;
	}

	public bool TestCommand(byte cmd) {
		byte[] message = new byte[3];
		message[0] = clientAddress;
		message[1] = (byte)cmd;
		SetChecksum(message);
		lock(this) {
			string cmdid = StartCommand("Command_"+cmd, 1, processStatusMessage);
			serialPort.Write(message,0, message.Length);		
			return WaitForResult(cmdid);
		}	
		
	}

	public bool HWStallProtectEnable(bool ena) {
		byte[] message = new byte[4];
		message[0] = clientAddress;
		message[1] = (byte)0x88;
		message[2] = ena ? (byte)1 : (byte)0;
		SetChecksum(message);
		lock(this) {
			string cmdid = StartCommand("SetStallProtectionEnable", 1, processStatusMessage);
			serialPort.Write(message,0, message.Length);		
			return WaitForResult(cmdid);
		}	
	}


	// position in pulses (different from encoderPosition)
	public bool Move(int deltaPosition,int speed) {
		byte[] message = new byte[8];
		message[0] = clientAddress;
		message[1] = (byte)0xFD;
		message[2] = 0;
		if (speed>=0) {
			message[2] = 0;
		} else {
			message[2] = 128;
		}
		speed = Math.Abs(speed);
		message[2] = (byte)(message[2] | (speed & 0x7f));

		message[3] = (byte)((deltaPosition>>24) &0xff);
		message[4] = (byte)((deltaPosition>>16) &0xff);
		message[5] = (byte)((deltaPosition>>8) &0xff);
		message[6] = (byte)(deltaPosition & 0xff);

		
		Logger.Debug($"GoToPosition s={message[2]},p: {message[3]}, {message[4]}, {message[5]}, {message[6]}");

		SetChecksum(message);
		lock(this) {
			string cmdid = StartCommand("Move", 1, processStatusMessage);
			constantSpeedEnabled = false;
			serialPort.Write(message,0, message.Length);		
			return WaitForResult(cmdid);
		}		
	}

	public bool FindLimit(int speed=10, int timeoutMs = 5000) {
		try {
			if (!StopMotor()) {
				Logger.Error("Could not stop motor");
				return false;
			}
			if (!HWStallProtectEnable(false)) {
				Logger.Error("Could not disable Stall protection");
				return false;
			}

			if (!PowerEnable(true)) {
				Logger.Error("Could not enable power");
				return false;
			}
			encoderRequestInterval = 100; // 10/s
			SetConstantSpeed(speed,true);
			
			
			Stopwatch timeoutCheck = new Stopwatch();
			timeoutCheck.Start();
			while ((timeoutCheck.ElapsedTicks / (Stopwatch.Frequency / (1000L)))<timeoutMs) {
				bool enabled;
				if (!GetPowerEnable(out enabled)) {
					Logger.Debug("FindLimit - could not check motor power");
					// error reading result
					return false;
				}
				if (!enabled) {
					// limit found
					Logger.Debug("FindLimit - ok");
					return true;
				}
				Thread.Sleep(500);
			}
			// timeout reached
			Logger.Debug("FindLimit timeout reached");
			return false;
		} finally {
			// TODO only enable if it was enabled when calling the method
			HWStallProtectEnable(true);
		}
	}	

	public bool SetConstantSpeed(int speed, bool guard) {
		byte[] message = new byte[4];
		message[0] = clientAddress;
		message[1] = (byte)0xF6;
		message[2] = 0;
		if (speed>=0) {
			message[2] = 0;
		} else {
			message[2] = 128;
		}
		speed = Math.Abs(speed);
		message[2] = (byte)(message[2] | (speed & 0x7f));

		SetChecksum(message);
		lock(this) {
			string cmdid = StartCommand("SetConstantSpeed", 1, processStatusMessage);
			constantSpeedSetpoint = speed;
			constantSpeedEnabled = true;
			constantSpeedGuard = guard;
			serialPort.Write(message,0, message.Length);		
			return WaitForResult(cmdid);
		}		
	}

	public bool getAngleError(out float _angleError) {
		byte[] message = new byte[3];
		message[0] = clientAddress;
		message[1] = (byte)0x39;
		SetChecksum(message);
		lock(this) {
			string cmdid = StartCommand("GetAngleError", 2, processAngleErrorMessage); 
			serialPort.Write(message,0, message.Length);		
			if (WaitForResult(cmdid)) {
				_angleError = angleError;
				return true;
			} else {
				_angleError = 0;
				return false;
			}
		}			
	}

	public bool getShaftStatus(out int _shaftStatus) {
		byte[] message = new byte[3];
		message[0] = clientAddress;
		message[1] = (byte)0x3e;
		SetChecksum(message);
		lock(this) {
			string cmdid = StartCommand("GetShaftStatus", 2, processMotorShaftStatusMessage); 
			serialPort.Write(message,0, message.Length);		
			if (WaitForResult(cmdid)) {
				_shaftStatus = shaftStatus;
				return true;
			} else {
				_shaftStatus = 0;
				return false;
			}
		}			
	}

	private byte CalculateChecksum(byte[] message) {
		byte checksum = 0;
		for (int i=0; i<message.Length-1; i++) {
			checksum += message[i];
		}
		return checksum;
	}

	private void SetChecksum(byte[] message) {
		byte checksum = CalculateChecksum(message);
		message[message.Length-1] = checksum;
	}

	private bool CheckMessage(byte[] message) {
		byte checksum = CalculateChecksum(message);
		byte actualChecksum = message[message.Length-1];
		return checksum == actualChecksum;
	}

	private bool WaitForResult(string _commandId) {
		int time = 0;
		string cmds = string.Join(",", executingCommands);
		Logger.Debug($"{commandId}: Wait for result (other commands {cmds}");
		while ((time<1000) && (executingCommands.Contains(_commandId))) {
			Thread.Sleep(1);
			time += 1;
		}	
		cmds = string.Join(",", executingCommands);
		Logger.Debug($"{_commandId}: Wait done {cmds}");
		if (executingCommands.Contains(_commandId)) {
			Logger.Debug($"{_commandId}: Timeout reading result");
			EndCommand(_commandId);
		}
		return readerState == ReadState.Decoded;
			
	}

	private void Monitor() {
		int tm = 0;
		while (!stopMonitor) {
			tm+=1;
			if (serialPort.IsOpen) {
				// not a very accurate way to measure time, but simple
				if (tm>encoderRequestInterval) {
					tm = 0;
					lock(this) {
						if (!stopMonitor) {
							RequestReadEncoder();
							if (stallDetected && powerEnabled) {
								Logger.Debug("Encoder read, stall detected, take action");
								PowerEnable(false);
								StopMotor();
							}
						}
					}
				}
			} else {
				Logger.Debug($"Serial port closed");
			}
			Thread.Sleep(1);
		}
		Logger.Debug($"Exited monitor");
	}

	private bool RequestReadEncoder() {
		Logger.Debug($"Auto Request encoder");
		byte[] message = new byte[3];
		message[0] = clientAddress;
		message[1] = 0x30;
		SetChecksum(message);
			
		string cmdid = StartCommand("RequestEncoder", 2, processEncoderMessage); 
		encoderRequestTime = encoderReadTimer.ElapsedTicks / (Stopwatch.Frequency / (1000L*1000L));
		serialPort.Write(message,0, message.Length);
		if (WaitForResult(cmdid)) {
			return true;
		} else {
			return false;
		}
	
	}
}


