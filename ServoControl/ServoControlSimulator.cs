using System.Threading;

namespace ServoControl;
public class ServoControllerSimulator : IServoController
{
    private bool _powerEnabled = false;
    private float _speed = 0;
    private float _absEncoderPosition = 12500;
    private int _reportedEncoderPositionDelta = -12500;
    private int _maxAbsEncoderPosition = 70000;
    private int _minAbsEncoderPosition = 0;

    private bool _stallDetected = false;
    private bool _constantSpeedGuard = false;
    private bool _constantSpeedEnabled = false;
    

    byte IServoController.clientAddress { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

    int IServoController.errorCount => throw new NotImplementedException();

    int IServoController.encoderRead => throw new NotImplementedException();

    public int absEncoderPosition { get => (int)_absEncoderPosition + _reportedEncoderPositionDelta; }

    float IServoController.estimatedEncoderSpeed => throw new NotImplementedException();

    public int encoderRequestInterval { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

    float IServoController.angleError => throw new NotImplementedException();

    int IServoController.shaftStatus => throw new NotImplementedException();

    public bool powerEnabled {get => _powerEnabled; }

    public bool constantSpeedEnabled { get => _constantSpeedEnabled;}

    public bool constantSpeedGuard { get => _constantSpeedGuard; }

    public bool stallDetected { get => _stallDetected; }

    int IServoController.debug { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

    public ServoControllerSimulator() {
        Thread backgroundThread = new Thread(new ThreadStart(Simulate));
        backgroundThread.Start();
    }

    private void Simulate() {
        long prevTime = 0;
        while (true) {
            long curTime = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;

            if (_constantSpeedEnabled) {
                // speed == 10 -> encoder changes 7000 /s -> 7 p / ms
                if (prevTime!=0) {
                    long dt = curTime - prevTime;
                    _absEncoderPosition += (_speed/10.0f)*7.0f*dt;
                    if (_absEncoderPosition > _maxAbsEncoderPosition) {
                        _absEncoderPosition = _maxAbsEncoderPosition;
                        if (_constantSpeedGuard) {
                            _stallDetected = true;
                        }
                    }
                    if (_absEncoderPosition < _minAbsEncoderPosition) {
                        _absEncoderPosition = _minAbsEncoderPosition; 
                        if (_constantSpeedGuard) {
                            _stallDetected = true;
                        }
                    }
                }
                prevTime = curTime;
                Thread.Sleep(1);
            } else {
                prevTime = curTime;
                Thread.Sleep(1);
            }

            if (stallDetected && powerEnabled) {
                StopMotor();
                PowerEnable(false);

            }

        }
    }

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

    public bool GetPowerEnable(out bool ena)
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

    public bool PowerEnable(bool ena)
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

    public void ReleaseSWProtection()
    {
        _stallDetected = false;
    }

    public bool SetConstantSpeed(int speed, bool guard)
    {
        if (!_powerEnabled) {
            return false;
        } else {
            this._constantSpeedGuard = guard;
            this._constantSpeedEnabled = true;
            //this._constantSpeedEnabledStartTime = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
            this._speed = speed;
            return true;
        }

    }

    public bool StopMotor()
    {
        if (!_powerEnabled) {
            return false;
        } else {
            _speed = 0;
            this._constantSpeedEnabled = false;
            return true;
        }
    }

    bool IServoController.TestCommand(byte cmd)
    {
        throw new NotImplementedException();
    }

    public void calibrateEncoderPosition(int EncoderPosition) {
		int reportedPosition = (int)_absEncoderPosition + _reportedEncoderPositionDelta;
        int delta = EncoderPosition - reportedPosition;
        _reportedEncoderPositionDelta += delta;
	}
}