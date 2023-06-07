namespace PowerSupplies.Core;

public interface IPowerSupply
{
    string Info { get; }

    bool Connect(string port);
    void Disconnect();

    void SetOutput(bool output);

    IEnumerable<IReadOnlyCurrentPoint> MeasureCurrent();
}