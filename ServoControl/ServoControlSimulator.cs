namespace ServoControl;
public class ServoControllerSimulator : IServoController
{
    private bool _powerEnabled = false;
    private float _speed = 0;
    private int _absEncoderPosition = 500;
    private int _reportedEncoderPositionDelta = -500;
    private int _maxAbsEncoderPosition = 1000;
    private int _minAbsEncoderPosition = 0;

    private bool _stallDetected = false;
    private bool _constantSpeedGuard = false;
    private bool _constantSpeedEnabled = false;

    byte IServoController.clientAddress { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

    int IServoController.errorCount => throw new NotImplementedException();

    int IServoController.encoderRead => throw new NotImplementedException();

    int IServoController.absEncoderPosition { get => _absEncoderPosition + _reportedEncoderPositionDelta; }

    float IServoController.estimatedEncoderSpeed => throw new NotImplementedException();

    int IServoController.encoderRequestInterval { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

    float IServoController.angleError => throw new NotImplementedException();

    int IServoController.shaftStatus => throw new NotImplementedException();

    bool IServoController.powerEnabled {get => _powerEnabled; }

    bool IServoController.constantSpeedEnabled { get => _constantSpeedEnabled;}

    bool IServoController.constantSpeedGuard { get => _constantSpeedGuard; }

    bool IServoController.stallDetected { get => _stallDetected; }

    int IServoController.debug { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

    void IServoController.Close()
    {
        throw new NotImplementedException();
    }

    bool IServoController.FindLimit(int speed, int timeoutMs)
    {
        throw new NotImplementedException();
    }

    bool IServoController.getAngleError(out float _angleError)
    {
        throw new NotImplementedException();
    }

    bool IServoController.GetPowerEnable(out bool ena)
    {
        ena = _powerEnabled;
        return true;
    }

    bool IServoController.getShaftStatus(out int _shaftStatus)
    {
        throw new NotImplementedException();
    }

    bool IServoController.HWStallProtectEnable(bool ena)
    {
        throw new NotImplementedException();
    }

    bool IServoController.Move(int deltaPosition, int speed)
    {
        throw new NotImplementedException();
    }

    void IServoController.Open()
    {
        // noop
    }

    bool IServoController.PowerEnable(bool ena)
    {
        _powerEnabled = ena;
        if (!ena) {
            _speed = 0;
            _constantSpeedEnabled = false;
        }
        return true;
    }

    bool IServoController.ReleaseHWProtection()
    {
        throw new NotImplementedException();
    }

    void IServoController.ReleaseSWProtection()
    {
        _stallDetected = false;
    }

    bool IServoController.SetConstantSpeed(int speed, bool guard)
    {
        throw new NotImplementedException();
    }

    bool IServoController.StopMotor()
    {
        if (!_powerEnabled) {
            return false;
        } else {
            _speed = 0;
            return true;
        }
    }

    bool IServoController.TestCommand(byte cmd)
    {
        throw new NotImplementedException();
    }

    public void calibrateEncoderPosition(int EncoderPosition) {
		int reportedPosition = _absEncoderPosition + _reportedEncoderPositionDelta;
        int delta = EncoderPosition - reportedPosition;
        _reportedEncoderPositionDelta += delta;
	}
}