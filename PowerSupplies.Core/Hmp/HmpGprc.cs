using Grpc.Net.Client;
using HmpService;

namespace PowerSupplies.Core.Hmp;

public class HmpGprc : IMultiChannelsPowerSupply
{
    public string Info { get; private set; } = string.Empty;

    ~HmpGprc()
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
        _id = Guid.Parse(ThrowIfError(_client!.Open(new OpenRequest { PortName = portName })).Id);
        Info = ThrowIfError(_client!.GetInfo(new InfoRequest())).Info;
    }

    public void Close()
    {
        ThrowIfError(_client!.Close(new CloseRequest()));
    }

    public double MeasureCurrent(int channel)
    {
        return ThrowIfError(_client!.MeasureCurrent(new MeasureCurrentRequest { Channel = channel, Id = _id.ToString() })).Value;
    }

    public void SetVoltageCurrent(double voltage, double current, int channel)
    {
        ThrowIfError(_client!.SetVoltageCurrent(new SetVoltageCurrentRequest
        {
            Id = _id.ToString(),
            Channel = channel,
            Current = current,
            Voltage = voltage
        }));
    }

    public void SetOutput(bool output, int channel)
    {
        ThrowIfError(_client!.SetOutput(new OutputRequest { Channel = channel, Output = output, Id = _id.ToString() }));
    }

    private static OpenReply ThrowIfError(OpenReply reply)
    {
        if (reply.Status != 0)
        {
            throw new Exception(reply.Error);
        }

        return reply;
    }

    private static Reply ThrowIfError(Reply reply)
    {
        if (reply.Status != 0)
        {
            throw new Exception(reply.Error);
        }

        return reply;
    }

    private static MeasureCurrentReply ThrowIfError(MeasureCurrentReply reply)
    {
        if (reply.Status != 0)
        {
            throw new Exception(reply.Error);
        }

        return reply;
    }

    private static InfoReply ThrowIfError(InfoReply reply)
    {
        if (reply.Status != 0)
        {
            throw new Exception(reply.Error);
        }

        return reply;
    }

    private Guid _id = Guid.Empty;
    private GrpcChannel? _channel;
    private HmpService.HmpService.HmpServiceClient? _client;
}