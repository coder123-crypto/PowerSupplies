using Grpc.Net.Client;
using PshService;

namespace PowerSupplies.Core.Psh;

public class PshGrpc : IPowerSupply
{
    public string Info { get; private set; } = string.Empty;

    ~PshGrpc()
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
        _id = Guid.Parse(ThrowIfError(_client!.Open(new OpenRequest { PortName = portName })).Id);
        Info = ThrowIfError(_client!.GetInfo(new InfoRequest { Id = _id.ToString() })).Info;
    }

    public void Close()
    {
        ThrowIfError(_client!.Close(new CloseRequest { Id = _id.ToString() }));
    }

    public double MeasureCurrent()
    {
        return ThrowIfError(_client!.MeasureCurrent(new MeasureCurrentRequest { Id = _id.ToString() })).Value;
    }

    public void SetVoltageCurrent(double voltage, double current)
    {
        ThrowIfError(_client!.SetVoltageCurrent(new SetVoltageCurrentRequest { Current = current, Voltage = voltage, Id = _id.ToString() }));
    }

    public void SetOutput(bool output)
    {
        ThrowIfError(_client!.SetOutput(new OutputRequest { Output = output, Id = _id.ToString() }));
    }

    private static OpenReply ThrowIfError(OpenReply reply)
    {
        if (reply.Status != 0)
        {
            throw new Exception(reply.Error);
        }

        return reply;
    }

    private static void ThrowIfError(Reply reply)
    {
        if (reply.Status != 0)
        {
            throw new Exception(reply.Error);
        }
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

    private GrpcChannel? _channel;
    private PshService.PshService.PshServiceClient? _client;
    private Guid _id = Guid.Empty;
}