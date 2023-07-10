using System.IO.Ports;
using static System.Globalization.CultureInfo;

namespace PowerSupplies.Core.Hmp;

public class Hmp : IMultiChannelsPowerSupply
{
    public string Info { get; private set; } = string.Empty;

    ~Hmp()
    {
        Close();
    }

    public void SetOutput(bool output, int channel)
    {
        if (channel == -1)
        {
            SetOutput(output ? 1 : 0);
        }
        else
        {
            SetOutput(channel, output ? 1 : 0);
        }
    }

    public void Close()
    {
        _port.Close();
    }

    public void Open(string portName)
    {
        _port.Close();

        _port.PortName = portName;
        _port.Open();

        WriteLine("*IDN?");
        string[] infos = _port.ReadLine().Split(",");
        Info = $"{infos[1]} №{infos[2]}";
        Wait();
        WriteLine("SYST:MIX");
    }

    public void SetVoltageCurrent(double voltage, double current, int channel)
    {
        Request("INST:NSEL", channel);
        Request("VOLT", voltage);
        Request("CURR", current);
    }

    public double MeasureCurrent(int channel)
    {
        Request("INST:NSEL", channel);
        return Request("MEAS:CURR");
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
}