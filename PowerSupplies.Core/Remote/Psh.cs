using Grpc.Net.Client;
using PshService;

namespace PowerSupplies.Core.Remote;

public class Psh : IPsh
{
    public string PshInfo { get; private set; } = string.Empty;

    ~Psh()
    {
        Close();
    }

    public void Connect(string address)
    {
        _channel?.Dispose();
        _channel = GrpcChannel.ForAddress(address);

        _client = new PshService.PshService.PshServiceClient(_channel);
    }

    public void Open(string portName)
    {
        ThrowIfError(_client!.Open(new Request { Content = portName }));

        var reply = _client!.GetInfo(new InfoRequest());
        ThrowIfError(reply);
        PshInfo = reply.Info;
    }

    public void Close()
    {
        ThrowIfError(_client!.Close(new Request()));
    }

    public double MeasureCurrent()
    {
        var reply = _client!.MeasureCurrent(new MeasureCurrentRequest());
        ThrowIfError(reply);
        return reply.Value;
    }

    public void SetVoltageCurrent(double voltage, double current)
    {
        ThrowIfError(_client!.SetVoltageCurrent(new SetVoltageCurrentRequest { Current = current, Voltage = voltage }));
    }

    public void SetOutput(bool output)
    {
        ThrowIfError(_client!.SetOutput(new OutputRequest { Output = output }));
    }

    private static void ThrowIfError(Reply reply)
    {
        if (reply.Status != 0)
        {
            throw new Exception(reply.Error);
        }
    }

    private static void ThrowIfError(MeasureCurrentReply reply)
    {
        if (reply.Status != 0)
        {
            throw new Exception(reply.Error);
        }
    }

    private static void ThrowIfError(InfoReply reply)
    {
        if (reply.Status != 0)
        {
            throw new Exception(reply.Error);
        }
    }

    private GrpcChannel? _channel;
    private PshService.PshService.PshServiceClient? _client;
}