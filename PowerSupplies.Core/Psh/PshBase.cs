namespace PowerSupplies.Core.Psh;

public abstract class PshBase : IPowerSupplyListener
{
    #region IPowerSupply
    public string Info => _psh?.Info ?? string.Empty;

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
            switch (_psh)
            {
                case PshSerial local:
                    local.Close();
                    break;

                case PshGrpc remote:
                    remote.Close();
                    break;
            }
        }
    }

    public void Connect(string connectionString)
    {
        Disconnect();

        lock (_locker)
        {
            if (connectionString.StartsWith("COM"))
            {
                var serial = new PshSerial();
                serial.Open(connectionString);
                _psh = serial;
            }
            else
            {
                string[] args = connectionString.Split(":");

                var proto = new PshGrpc();
                proto.Connect($"http://{args[0]}:{args[1]}");
                proto.Open(args[2]);
                _psh = proto;
            }

        }
    }

    private DateTime _startedTime = DateTime.Now;
    private readonly object _locker = new();
    private IPowerSupply? _psh;
}