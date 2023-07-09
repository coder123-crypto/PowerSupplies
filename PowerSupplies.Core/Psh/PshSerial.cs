using System.IO.Ports;
using static System.Globalization.CultureInfo;

namespace PowerSupplies.Core.Psh;

public class PshSerial : IPowerSupply
{
    public string Info { get; private set; } = string.Empty;

    ~PshSerial()
    {
        Close();
    }

    public double MeasureCurrent()
    {
        _port.WriteLine("CHAN1:MEAS:CURR ?");
        return double.Parse(_port.ReadLine().Trim(), InvariantCulture);
    }

    public void SetVoltageCurrent(double voltage, double current)
    {
        _port.WriteLine($"CHAN1: VOLT {voltage.ToString(InvariantCulture)}; CURR {current.ToString(InvariantCulture)}");
    }

    public void SetOutput(bool output)
    {
        _port.WriteLine(output ? ":OUTPut:STATe 1" : ":OUTPut:STATe 0");
    }

    public void Close()
    {
        _port.Close();
    }

    public void Open(string port)
    {
        Close();

        _port.PortName = port;
        _port.Open();
        _port.WriteLine("*IDN?");
        string line = _port.ReadLine();
        var args = line.Split(',');
        Info = $"{args[1]} №{args[2]}";
    }

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