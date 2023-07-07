using Grpc.Net.Client;
using HmpService;

namespace PowerSupplies.Core.Remote;

public class HmpBase : IHmp
{
    public string HmpInfo { get; private set; } = string.Empty;

    ~HmpBase()
    {
        Close();
    }

    public void Connect(string address)
    {
        _channel?.Dispose();
        _channel = GrpcChannel.ForAddress(address);

        _client = new HmpService.HmpService.HmpServiceClient(_channel);
    }

    public void Open(string portName)
    {
        ThrowIfError(_client!.Open(new Request {Content = portName}));

        var reply = _client!.GetInfo(new InfoRequest());
        ThrowIfError(reply);
        HmpInfo = reply.Info;
    }

    public void Close()
    {
        ThrowIfError(_client!.Close(new Request()));
    }

    public double MeasureCurrent(int channel)
    {
        var reply = _client!.MeasureCurrent(new MeasureCurrentRequest { Channel = channel });
        ThrowIfError(reply);
        return reply.Value;
    }

    public void SetVoltageCurrent(double voltage, double current, int channel)
    {
        ThrowIfError(_client!.SetVoltageCurrent(new SetVoltageCurrentRequest
        {
            Channel = channel,
            Current = current,
            Voltage = voltage
        }));
    }

    public void SetOutput(bool output, int channel)
    {
        ThrowIfError(_client!.SetOutput(new OutputRequest { Channel = channel, Output = output }));
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
    private HmpService.HmpService.HmpServiceClient? _client;
}