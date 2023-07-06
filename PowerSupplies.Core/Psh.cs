using Serial2Network.Core;
using System.IO.Ports;
using static System.Globalization.CultureInfo;

namespace PowerSupplies.Core;

public sealed class Psh : IPowerSupply
{
    public string Info
    {
        get
        {
            lock (_locker)
            {
                _client.WriteLine("*IDN?");

                string line = _client.ReadLine();
                var args = line.Split(',');
                return $"{args[1]} №{args[2]}";
            }
        }
    }

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
            _client.WriteLine("CHAN1:MEAS:CURR ?");
            yield return new CurrentPoint(DateTime.Now - _startedTime, double.Parse(_client.ReadLine().Trim(), InvariantCulture));
        }
    }

    public void SetVoltageCurrent(double voltage, double current)
    {
        lock (_locker)
        {
            _client.WriteLine($"CHAN1: VOLT {voltage.ToString(InvariantCulture)}; CURR {current.ToString(InvariantCulture)}");
        }
    }

    public void SetOutput(bool output)
    {
        lock (_locker)
        {
            _client.WriteLine(output ? ":OUTPut:STATe 1" : ":OUTPut:STATe 0");
        }
    }

    public void Disconnect()
    {
        lock (_locker)
        {
            _client.Disconnect();
        }
    }

    public bool Connect(string port)
    {
        try
        {
            Disconnect();

            lock (_locker)
            {
                _client.Connect(port);
                _client.WriteLine("*IDN?");
                _ = _client.ReadLine();
            }
        }
        catch (Exception)
        {
        }

        try
        {
            lock (_locker)
            {
                _client.WriteLine("*IDN?");

                string line = _client.ReadLine();
                if (!line.StartsWith("GW", StringComparison.Ordinal))
                {
                    return false;
                }
            }
        }
        catch (TimeoutException)
        {
            return false;
        }

        return true;
    }

    private DateTime _startedTime = DateTime.Now;
    private readonly object _locker = new();
    private readonly Client _client = new()
    {
        SerialPortOptions =
        {
            BaudRate = 9600,
            StopBits = StopBits.One,
            Handshake = Handshake.None,
            Parity = Parity.None,
            WriteTimeout = 1000,
            DataBits = 8,
            ReadBufferSize = 4096,
            ReadTimeout = 1000,
            WriteBufferSize = 4096
        }
    };
}