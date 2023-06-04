using System.IO.Ports;
using static System.Globalization.CultureInfo;

namespace PowerSupplies.Core;

public sealed class Psh
{
    public string PshInfo
    {
        get
        {
            lock (_locker)
            {
                _port.WriteLine("*IDN?");

                string line = _port.ReadLine();
                var args = line.Split(',');
                return $"{args[1]} №{args[2]}";
            }
        }
    }

    ~Psh()
    {
        Disconnect();
    }

    public double MeasureCurrent()
    {
        lock (_locker)
        {
            _port.WriteLine("CHAN1:MEAS:CURR ?");
            return double.Parse(_port.ReadLine().Trim(), InvariantCulture);
        }
    }

    public void SetVoltageCurrent(double voltage, double current)
    {
        lock (_locker)
        {
            _port.WriteLine($"CHAN1: VOLT {voltage.ToString(InvariantCulture)}; CURR {current.ToString(InvariantCulture)}");
        }
    }

    public void SetOutput(bool output)
    {
        lock (_locker)
        {
            _port.WriteLine(output ? ":OUTPut:STATe 1" : ":OUTPut:STATe 0");
        }
    }

    public void Disconnect()
    {
        lock (_locker)
        {
            _port.Close();
        }
    }

    public bool Connect(string port)
    {
        try
        {
            Disconnect();

            lock (_locker)
            {
                _port.PortName = port;
                _port.Open();
                _port.WriteLine("*IDN?");
                _port.ReadLine();
            }
        }
        catch (TimeoutException)
        {
        }

        try
        {
            lock (_locker)
            {
                _port.WriteLine("*IDN?");

                string line = _port.ReadLine();
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

    private readonly object _locker = new();
    private readonly SerialPort _port = new()
    {
        BaudRate = 9600,
        DataBits = 8,
        Parity = Parity.None,
        StopBits = StopBits.One,
        Handshake = Handshake.None,
        ReadTimeout = 1000,
        WriteTimeout = 1000,
        ReadBufferSize = 4096,
        WriteBufferSize = 4096
    };
}