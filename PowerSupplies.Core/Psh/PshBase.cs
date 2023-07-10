namespace PowerSupplies.Core.Psh;

public abstract class PshBase : IPowerSupplyListener
{
    #region IPowerSupply

    public string Info
    {
        get
        {
            lock (_locker)
            {
                return _psh?.Info ?? string.Empty;
            }
        }
    }

    public IEnumerable<IReadOnlyCurrentPoint> MeasureCurrent()
    {
        lock (_locker)
        {
            yield return new CurrentPoint(DateTime.Now - _startedTime, _psh!.MeasureCurrent());
        }
    }

    public void SetVoltageCurrent(double voltage, double current)
    {
        if (voltage > _maximumVoltage)
        {
            voltage = _maximumVoltage;
        }

        if (current > _maximumCurrent)
        {
            current = _maximumCurrent;
        }

        if (voltage * current > _maximumPower)
        {
            current = _maximumPower / voltage;
        }

        lock (_locker)
        {
            _psh!.SetVoltageCurrent(voltage, current);
        }
    }

    public void SetOutput(bool output)
    {
        lock (_locker)
        {
            _psh!.SetOutput(output);
        }
    }
    #endregion

    protected PshBase(double maximumVoltage, double maximumCurrent, double maximumPower)
    {
        _maximumVoltage = maximumVoltage;
        _maximumCurrent = maximumCurrent;
        _maximumPower = maximumPower;
    }
    private readonly double _maximumVoltage;
    private readonly double _maximumCurrent;
    private readonly double _maximumPower;

    ~PshBase()
    {
        Disconnect();
    }

    public void ResetTimer()
    {
        _startedTime = DateTime.Now;
    }

    public void Disconnect()
    {
        lock (_locker)
        {
            _psh?.Close();
        }
    }

    public void Connect(string portName)
    {
        Disconnect();

        lock (_locker)
        {
            _psh = new Psh();
            _psh.Open(portName);
        }
    }

    private DateTime _startedTime = DateTime.Now;
    private readonly object _locker = new();
    private Psh? _psh;
}