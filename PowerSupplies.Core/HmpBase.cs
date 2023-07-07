namespace PowerSupplies.Core;

public abstract class HmpBase : IPowerSupply
{
    public string Info => _hmp?.HmpInfo ?? string.Empty;

    public HmpBase(int channels, params double[] maximumPowers)
    {
        _channels = channels;
        _maximumPowers = maximumPowers;
    }
    private readonly int _channels;
    private readonly double[] _maximumPowers;

    public void ResetTimer()
    {
        _startedTime = DateTime.Now;
    }

    ~HmpBase()
    {
        _hmp?.Close();
    }

    public IEnumerable<IReadOnlyCurrentPoint> MeasureCurrent()
    {
        lock (_locker)
        {
            for (int i = 0; i < _channels; i++)
            {
                yield return new CurrentPoint(DateTime.Now - _startedTime, _hmp!.MeasureCurrent(i + 1));
            }
        }
    }

    public void SetOutput(bool output)
    {
        lock (_locker)
        {
            for (int i = 0; i < _channels; i++)
            {
                _hmp!.SetOutput(output, i + 1);
            }
            _hmp!.SetOutput(output, -1);
        }
    }

    public void Disconnect()
    {
        lock (_locker)
        {
            _hmp?.Close();
        }
    }

    public void Connect(string connectionString)
    {
        Disconnect();

        lock (_locker)
        {
            if (connectionString.StartsWith("COM"))
            {
                var serial = new Local.HmpBase();
                serial.Open(connectionString);
                _hmp = serial;
            }
            else
            {
                string[] args = connectionString.Split(":");

                var proto = new Remote.HmpBase();
                proto.Connect($"http://{args[0]}:{args[1]}");
                proto.Open(args[2]);
                _hmp = proto;
            }
        }
    }

    protected void SetVoltageCurrent(int channel, double voltage, double current)
    {
        if (voltage * current > _maximumPowers[channel - 1])
        {
            current = _maximumPowers[channel - 1] / voltage;
        }

        lock (_locker)
        {
            _hmp!.SetVoltageCurrent(voltage, current, channel);
        }
    }

    private DateTime _startedTime = DateTime.Now;
    private IHmp? _hmp;
    private readonly object _locker = new();
}