using System;
using System.IO.Ports;
using System.Threading;
using NLog;
using NLog.Config;
using NLog.Targets;

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
	private ReadState readerState = ReadState.ClientID;
	private byte readerChecksum = 0;
	private byte[] readerData = new byte[255];
	private int readerDataPosition = 0;

	private bool waitForEncoderResult = false;
	private bool waitForStatusResult = false;
	private bool stopMonitor = false;

	public byte clientAddress { get; set; } = 0xE0;
	public int errorCount { get; private set;} = 0;
	
	public int encoderRead { get; private set;} = 0;
	public int position { get; private set; } =0;
	public int encoderRequestInterval { get; set; } = 2000;
	public int debug { get; set; } = 0;

	private LoggingConfiguration logConfig = new NLog.Config.LoggingConfiguration();
	private NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();
	
	public ServoController() {
		serialPort = new SerialPort();	
		statusMonitor = new Thread(new ThreadStart(Monitor));

		// Targets where to log to: File and Console
		//var logconsole = new NLog.Targets.ConsoleTarget("logconsole");
			    
		// Rules for mapping loggers to targets            
		//config.AddRule(LogLevel.Info, LogLevel.Fatal, logconsole);
		//config.AddRule(LogLevel.Debug, LogLevel.Info, LogLevel.Fatal, logfile);
			    
		// Apply config           
		NLog.LogManager.Configuration = logConfig;

		//NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();
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
		serialPort.Close();
		stopMonitor = true;
		
	}

	private (int, bool, bool) processReceivedData(byte[] in_buffer, int expectedLength) {
		int i = 0;
		while (i<in_buffer.Length) {
			if (readerState == ReadState.ClientID) {
				if (in_buffer[i] == clientAddress) {
					Console.WriteLine("Received client id");
					readerChecksum = clientAddress;
					readerDataPosition=0;
					readerState = ReadState.Data;
				}
				i+=1;
			} else if (readerState == ReadState.Data) {
				if (readerDataPosition<expectedLength) {
					Console.WriteLine($"Received data {readerDataPosition}/{expectedLength}");
					readerData[readerDataPosition] = in_buffer[i];
					readerChecksum += in_buffer[i];
					readerDataPosition++;
					i+=1;
				} else {
					readerState = ReadState.Checksum;
					if (readerChecksum == in_buffer[i]) {
						Console.WriteLine("Checksum ok");
						readerState = ReadState.Success;
						i+=1;
						return (i, true, false);
					} else {
						Console.WriteLine("Checksum err");
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
		Console.WriteLine("Data Received ", avail);
		
		byte[] in_buffer = new byte[avail];
		int actual = sp.Read(in_buffer,0, avail);

		Console.WriteLine("Data Read :", actual);
		
		if (waitForEncoderResult) {
			(int pos, bool success, bool failure) = processReceivedData(in_buffer, 2);
			if (success) {
				processEncoderMessage();
			}
			if (failure) {
				Console.WriteLine("Failed to read encoder message");
			}
		} else if (waitForStatusResult) {
			(int pos, bool success, bool failure) = processReceivedData(in_buffer, 1);	
			if (success) {
				processStatusMessage();
			}
			if (failure) {
				Console.WriteLine("Failed to read status message");
			}
		}
	}

	private void processEncoderMessage() {
		waitForEncoderResult = false;
		Console.WriteLine("Process enc message");
		if (readerDataPosition==2) {
			readerState = ReadState.Decoded;
			Console.WriteLine($"Process encoder message: {readerData[0]},{readerData[1]}");
			encoderRead+=1;
		} else {
			readerState = ReadState.Failure;
			errorCount+=1;
			Console.WriteLine("Process encoder message - incorrect length");
		}
	}

	private void processStatusMessage() {
		waitForStatusResult = false;
		Console.WriteLine("Process status message");
			
		if (readerDataPosition==1) {
			if (readerData[0] == 1) {
				readerState = ReadState.Decoded;
				Console.WriteLine("Process status message - success");
			} else {
				readerState = ReadState.Failure;
				errorCount+=1;
				Console.WriteLine("Process status - failure");
			}
		} else {
			readerState = ReadState.Failure;
			errorCount+=1;
			Console.WriteLine("Process status message - incorrect length");
		}
	}
	
	public bool StopMotor() {
		byte[] message = new byte[3];
		message[0] = clientAddress;
		message[1] = (byte)0xF7;
		SetChecksum(message);
		lock(this) {
			readerState = ReadState.ClientID;
			waitForStatusResult = true;

			Console.WriteLine("Send Stop Motor");
			serialPort.Write(message,0, message.Length);
			return WaitForStatusResult();
		}
	}

	public bool PowerEnable(bool ena) {
		byte[] message = new byte[4];
		message[0] = clientAddress;
		message[1] = (byte)0xF3;
		message[2] = ena ? (byte)1 : (byte)0;
		SetChecksum(message);
		lock(this) {
			readerState = ReadState.ClientID;
			waitForStatusResult = true;
			
			serialPort.Write(message,0, message.Length);		
			return WaitForStatusResult();
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

	private bool WaitForStatusResult() {
		int time = 0;
		while ((time<5) && waitForStatusResult) {
			Thread.Sleep(1000);
			time += 1;
		}	
		if (waitForStatusResult) {
			Console.WriteLine("Timeout reading status result");
			waitForStatusResult = false;
		}
		return readerState == ReadState.Decoded;
			
	}

	private bool WaitForEncoderResult() {
		int time = 0;
		
		while ((time<5000) && waitForEncoderResult) {
			Thread.Sleep(1);
			time += 1;
		}	
		if (waitForEncoderResult) {
			Console.WriteLine("Timeout reading encoder result");
			waitForEncoderResult = false;
		}
		return readerState == ReadState.Decoded;
	}


	private void Monitor() {
		while (!stopMonitor) {
			if (serialPort.IsOpen) {
				lock(this) {
					RequestReadEncoder();
					WaitForEncoderResult();
				}
			} else {
				Console.WriteLine("Serial port closed");
			}
			Thread.Sleep(encoderRequestInterval);
		}
		Console.WriteLine("Exited monitor");
	}

	private void RequestReadEncoder() {
		Console.WriteLine("Auto Request encoder");
		byte[] message = new byte[3];
		message[0] = clientAddress;
		message[1] = 0x30;
		SetChecksum(message);
		
		readerState = ReadState.ClientID;
		waitForEncoderResult = true;

		serialPort.Write(message,0, message.Length);
			
	}
}


