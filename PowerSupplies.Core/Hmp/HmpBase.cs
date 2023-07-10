namespace PowerSupplies.Core.Hmp;

public abstract class HmpBase : IPowerSupplyListener
{
    #region IMultiChannelsPowerSupply
    public string Info => _hmp?.Info ?? string.Empty;

    public IReadOnlyCurrentPoint MeasureCurrent(int channel)
    {
        lock (_locker)
        {
            return new CurrentPoint(DateTime.Now - _startedTime, _hmp!.MeasureCurrent(channel));
        }
    }

    public void SetVoltageCurrent(double voltage, double current, int channel)
    {
        if (voltage > _maximumVoltages[channel - 1])
        {
            voltage = _maximumVoltages[channel - 1];
        }

        if (current > _maximumCurrents[channel - 1])
        {
            current = _maximumCurrents[channel - 1];
        }

        if (voltage * current > _maximumPowers[channel - 1])
        {
            current = _maximumPowers[channel - 1] / voltage;
        }

        lock (_locker)
        {
            _hmp!.SetVoltageCurrent(voltage, current, channel);
        }
    }

    public void SetOutput(bool output, int channel)
    {
        lock (_locker)
        {
            _hmp!.SetOutput(output, channel);
        }
    }
    #endregion

    public HmpBase(double[] maximumVoltages, double[] maximumCurrents, double[] maximumPowers)
    {
        _channels = maximumVoltages.Length;
        _maximumVoltages = maximumVoltages;
        _maximumCurrents = maximumCurrents;
        _maximumPowers = maximumPowers;
    }
    private readonly int _channels;
    private readonly double[] _maximumVoltages;
    private readonly double[] _maximumCurrents;
    private readonly double[] _maximumPowers;

    public void ResetTimer()
    {
        _startedTime = DateTime.Now;
    }

    ~HmpBase()
    {
        Disconnect();
    }

    public IEnumerable<IReadOnlyCurrentPoint> MeasureCurrent()
    {
        lock (_locker)
        {
            for (int i = 0; i < _channels; i++)
            {
                yield return MeasureCurrent(_channels + 1);
            }
        }
    }

    public void Disconnect()
    {
        lock (_locker)
        {
            _hmp?.Close();
        }
    }

    public void Connect(string portName)
    {
        Disconnect();

        lock (_locker)
        {
            _hmp = new Hmp();
            _hmp.Open(portName);
        }
    }

    private DateTime _startedTime = DateTime.Now;
    private Hmp? _hmp;
    private readonly object _locker = new();
}