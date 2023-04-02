# MKS_SERVO42C
C# library for MKS Servo42C V1.1

https://github.com/makerbase-mks/MKS-SERVO42C

After initial tests I don't advise to buy this item if you plan to use UART mode. There are some serious disadvantages:
1. There is a command to perform a number of steps. However during the time the motor is busy any command sent to the motor will cause the movement of the motor to stop. So there is no way to check if the command is done. (The constant speed command can be used to set the motor to run at a constant speed. While this command runs you can use the encoder messages to monitor the actual position of the motor).
2. The encoder message of the V1.1. motor is 16 bits. You have to regularly read the encoder to correctly detect wrap around. They seem to have fixed this in V1.1.2. But there is no way to update the firmware as this has not been released!




