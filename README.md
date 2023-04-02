# MKS_SERVO42C
C# library for MKS Servo42C V1.1

https://github.com/makerbase-mks/MKS-SERVO42C

After initial tests I don't advise to buy this item if you plan to use UART mode. There are some serious disadvantages:
1. There is a command to perform a number of steps. However during the time the motor is busy any command sent to the motor will cause the movement of the motor to stop. So there is no way to check if the command is done. (The constant speed command can be used to set the motor to run at a constant speed. While this command runs you can use the encoder messages to monitor the actual position of the motor).
2. The encoder message of the V1.1. motor is 16 bits. You have to regularly read the encoder to correctly detect wrap around. They seem to have fixed this in V1.1.2. But there is no way to update the firmware as this has not been released!
2. There is a command to protect against stall. However if the stall protection is triggered the motor no longer accepts commands via UART. You will have to press a button to release the motor. The release command via UART doesn't work.

Observations:
1. With mstep=256 when giving the motor a command to move 10000 pulses the encoder value changes by 12846
2. With mstep=256 and speed=10 the encoder changes by about 7000 per second.
3. With mstep=256 a revolution seems to equal 52000 pulses.

