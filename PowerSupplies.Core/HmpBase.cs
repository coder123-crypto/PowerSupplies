using System.IO.Ports;
using static System.Globalization.CultureInfo;

namespace PowerSupplies.Core;

public abstract class HmpBase : IPowerSupply
{
    public string Info
    {
        get
        {
            WriteLine("*IDN?");
            var args = _port.ReadLine().Split(',');
            string info = $"{args[1]} #{args[2]}";
            Wait();
            return info;
        }
    }

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
        for (int i = 0; i < _channels; i++)
        {
            _event.WaitOne();
            yield return new CurrentPoint(DateTime.Now - _startedTime, MeasureCurrentRequest(i));
            _event.Set();
        }
    }

    public void SetOutput(bool output)
    {
        _event.WaitOne();

        for (int i = 0; i < _channels; i++)
        {
            SetOutput(i + 1, output ? 1 : 0);
        }
        SetOutput(output ? 1 : 0);

        _event.Set();
    }

    public void Disconnect()
    {
        _event.WaitOne();
        _port.Close();
        _event.Set();
    }

    public bool Connect(string port)
    {
        _event.WaitOne();

        try
        {
            Disconnect();

            _event.WaitOne();
            _port.PortName = port;
            _port.Open();
            _event.Set();

            WriteLine("*IDN?");
            _ = _port.ReadLine();
            Wait();
            WriteLine("SYST:MIX");
        }
        catch (TimeoutException)
        {
            _event.Set();
            return false;
        }

        _event.Set();

        return true;
    }

    protected void SetVoltageCurrent(int channel, double voltage, double current)
    {
        if (voltage * current > _maximumPowers[channel - 1])
        {
            current = _maximumPowers[channel - 1] / voltage;
        }

        _event.WaitOne();
        Request("INST:NSEL", channel);
        Request("VOLT", voltage);
        Request("CURR", current);
        _event.Set();
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
        int answer = int.Parse(_port.ReadLine().Trim());
        if (answer != value)
        {
            throw new Exception($"Запрос {request} {value} не выполнен");
        }
        Wait();
    }

    private void Request(string request, double value)
    {

        WriteLine($"{request} {value.ToString(InvariantCulture)}");
        Wait();

        WriteLine($"{request}?");
        double answer = double.Parse(_port.ReadLine().Trim(), InvariantCulture);
        if (Math.Abs(answer - value) > 0.05)
        {
            throw new Exception($"Запрос {request} {value} не выполнен");
        }
        Wait();
    }

    private double Request(string request)
    {
        WriteLine($"{request}?");
        double r = double.Parse(_port.ReadLine().Trim(), InvariantCulture);
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
            _port.ReadExisting();

            try
            {
                WriteLine("*OPC?");
                resp = int.Parse(_port.ReadLine().Trim());
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

        _port.WriteLine(text);
    }

    private DateTime _startedTime = DateTime.Now;
    private DateTime _lastTimeWriteToPort = DateTime.Now;
    private readonly TimeSpan _writeDelay = TimeSpan.FromMilliseconds(50);
    private readonly SerialPort _port = new()
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
    };
    private readonly AutoResetEvent _event = new(true);
}