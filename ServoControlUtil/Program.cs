// See https://aka.ms/new-console-template for more information
using ServoControl;
using System;
using CommandLine;
using System.Threading;

namespace ServoControlUtil {

	public class Options {
		[Option('p', "port", Required = true, HelpText = "Serial port to connect to")]
		public string Port { get; set; } = "/dev/ttyUSB0";

		[Option('c', "command", Required = true, HelpText = "Motor command: Stop, Disable, Enable, GetPos")]
		public string Command { get; set; } = "GetPos";

		[Option('w', "wait", Required = false, HelpText = "  GetPos: keep reading encoder for 'wait' ms")]
		public int Wait { get; set; } = 0;

		[Option('e', "encoderRequestInterval", Required = false, HelpText = "Set the encoder request interval to x ms.")]
		public int encoderRequestInterval { get; set; } = 0;
		
		[Option('d', "debug", Required = false, HelpText = "Debug")]
		public bool debug { get; set; } = false;
	}

	class Program {
		static int Main(string[] args) {
			Options o = Parser.Default.ParseArguments<Options>(args).Value;
			ServoController c = new ServoController();
			if (o.debug) {
				c.debug = 2;
			} else {
				c.debug = 0;
			}
			c.Open();
			
			try {
				if (o.encoderRequestInterval !=0) {
					c.encoderRequestInterval = o.encoderRequestInterval;	
				}
				if (o.Command == "Stop") {
					Console.WriteLine("Stopping motor");
					if (c.StopMotor()) {
						Console.WriteLine("Motor stopped");
						return 0;
					} else {
						Console.WriteLine("Failed to stop motor");
						return 1;
					}
				}
				if (o.Command == "Disable") {
					Console.WriteLine("Disabling motor");
					if (c.PowerEnable(false)) {
						Console.WriteLine("Motor disabled");
						return 0;
					} else {
						Console.WriteLine("Failed to disable motor");
						return 1;
					}
				}
				if (o.Command == "Enable") {
					Console.WriteLine("Enabling motor");
					if (c.PowerEnable(true)) {
						Console.WriteLine("Motor enabled");
						return 0;
					} else {
						Console.WriteLine("Failed to enable motor");
						return 1;
					}
				}



				if (o.Command == "GetPos") {
					Console.WriteLine("Getting motor position");
					int w = 0;
					while ( ((c.encoderRead ==0) && (w<100)) || ((w*100)<o.Wait))  {
						Thread.Sleep(100);
						w+=1;
					}
					if (c.encoderRead == 0) {
						Console.WriteLine("Failed to get encoder position");
						return 1;
					} else {
						Console.WriteLine(c.position);
						return 0;
					}
				}
				Console.WriteLine("Unknown command");
				return 1;	
				//Console.WriteLine("Wait2");
				//Thread.Sleep(5000);
			} finally {
				c.Close();
			}
		}
	}
}
