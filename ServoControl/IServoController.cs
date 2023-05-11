namespace ServoControl;

public interface IServoController
{
    byte clientAddress { get; set; }
    int errorCount { get; }
    int encoderRead { get; }
    int absEncoderPosition { get; }
    float estimatedEncoderSpeed { get; }
    int encoderRequestInterval { get; set; }
    float angleError { get; }
    int shaftStatus { get; }
    bool powerEnabled { get; }
    bool constantSpeedEnabled { get; }
    bool constantSpeedGuard { get; }
    bool stallDetected { get; }
    int debug { get; set; }

    void Close();
    bool FindLimit(int speed = 10, int timeoutMs = 5000);
    bool getAngleError(out float _angleError);
    bool GetPowerEnable(out bool ena);
    bool getShaftStatus(out int _shaftStatus);
    bool HWStallProtectEnable(bool ena);
    bool Move(int deltaPosition, int speed);
    void Open();
    bool PowerEnable(bool ena);
    bool ReleaseHWProtection();
    void ReleaseSWProtection();
    bool SetConstantSpeed(int speed, bool guard);
    bool StopMotor();
    bool TestCommand(byte cmd);

    void calibrateEncoderPosition(int position);
}
