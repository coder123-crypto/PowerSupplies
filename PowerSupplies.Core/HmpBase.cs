using System.IO.Ports;
using System.Threading.Channels;
using Serial2Network.Core;
using static System.Globalization.CultureInfo;

namespace PowerSupplies.Core;

public abstract class HmpBase : IPowerSupply
{
    public string Info
    {
        get
        {
            var args = _info.Split(',');
            return $"{args[1]} #{args[2]}";
        }
    }
    private string _info = string.Empty;

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
        Disconnect();
    }

    public IEnumerable<IReadOnlyCurrentPoint> MeasureCurrent()
    {
        lock (_locker)
        {
            for (int channel = 0; channel < _channels; channel++)
            {
                yield return new CurrentPoint(DateTime.Now - _startedTime, MeasureCurrentRequest(channel + 1));
            }
        }
    }

    public void SetOutput(bool output)
    {
        lock (_locker)
        {
            for (int channel = 0; channel < _channels; channel++)
            {
                SetOutput(channel + 1, output ? 1 : 0);
            }
            SetOutput(output ? 1 : 0);
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
                WriteLine("*IDN?");
                _ = _client.ReadLine();
                Wait();
                WriteLine("SYST:MIX");
            }
        }
        catch (Exception)
        {
            return false;
        }
        
        try
        {
            lock (_locker)
            {
                WriteLine("*IDN?");
                _info = _client.ReadLine();
                if (!_info.StartsWith("HAMEG"))
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

    protected void SetVoltageCurrent(int channel, double voltage, double current)
    {
        if (voltage * current > _maximumPowers[channel - 1])
        {
            current = _maximumPowers[channel - 1] / voltage;
        }

        lock (_locker)
        {
            Request("INST:NSEL", channel);
            Request("VOLT", voltage);
            Request("CURR", current);
        }
    }

    private void SetOutput(int channel, int output)
    {
        Request("INST:NSEL", channel);
        Request("OUTP:SEL", output);
    }

    private void SetOutput(int output)
    {
        Request("OUTP", output);
    }

    private void Request(string request, int value)
    {
        WriteLine($"{request} {value}");
        Wait();

        WriteLine($"{request}?");
        int answer = int.Parse(_client.ReadLine().Trim());
        if (answer != value)
        {
            throw new Exception($"Запрос {request} {value} не выполнен");
        }
        Wait();
    }

    private void Request(string request, double value)
    {

        WriteLine($"{request} {value.ToString("F2", InvariantCulture)}");
        Wait();

        WriteLine($"{request}?");
        double answer = double.Parse(_client.ReadLine().Trim(), InvariantCulture);
        if (Math.Abs(answer - value) > 0.05)
        {
            throw new Exception($"Запрос {request} {value} не выполнен");
        }
        Wait();
    }

    private double Request(string request)
    {
        WriteLine($"{request}?");
        double r = double.Parse(_client.ReadLine().Trim(), InvariantCulture);
        Wait();
        return r;
    }

    private double MeasureCurrentRequest(int channel)
    {
        Request("INST:NSEL", channel);
        return Request("MEAS:CURR");
    }

    private void Wait()
    {
        int resp = -1;
        int count = 0;

        while (resp != 1 && count < 10)
        {
            _client.ReadExisting();

            try
            {
                WriteLine("*OPC?");
                resp = int.Parse(_client.ReadLine().Trim());
            }
            catch (Exception)
            {
                // ignored
            }

            count++;
        }
    }

    private void WriteLine(string text)
    {
        while (DateTime.Now - _lastTimeWriteToPort < _writeDelay)
        {
            Thread.Sleep(10);
        }
        _lastTimeWriteToPort = DateTime.Now;

        _client.WriteLine(text);
    }

    private DateTime _startedTime = DateTime.Now;
    private DateTime _lastTimeWriteToPort = DateTime.Now;
    private readonly TimeSpan _writeDelay = TimeSpan.FromMilliseconds(50);
    private readonly Client _client = new()
    {
        SerialPortOptions =
        {
            BaudRate = 9600,
            StopBits = StopBits.One,
            Handshake = Handshake.None,
            Parity = Parity.None,
            WriteTimeout = 5000,
            DataBits = 8,
            ReadBufferSize = 4096,
            ReadTimeout = 5000,
            WriteBufferSize = 4096
        }
    };
    private readonly object _locker = new();
}