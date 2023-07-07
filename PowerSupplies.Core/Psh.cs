namespace PowerSupplies.Core;

public sealed class Psh : IPowerSupply
{
    public string Info => _psh?.PshInfo ?? string.Empty;

    ~Psh()
    {
        Disconnect();
    }

    public void ResetTimer()
    {
        _startedTime = DateTime.Now;
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

    public void Disconnect()
    {
        lock (_locker)
        {
            _psh?.Close();
        }
    }

    public void Connect(string connectionString)
    {
        Disconnect();

        lock (_locker)
        {
            if (connectionString.StartsWith("COM"))
            {
                var serial = new Local.Psh();
                serial.Open(connectionString);
                _psh = serial;
            }
            else
            {
                string[] args = connectionString.Split(":");

                var proto = new Remote.Psh();
                proto.Connect($"http://{args[0]}:{args[1]}");
                proto.Open(args[2]);
                _psh = proto;
            }

        }
    }

    private DateTime _startedTime = DateTime.Now;
    private readonly object _locker = new();
    private IPsh? _psh;
}