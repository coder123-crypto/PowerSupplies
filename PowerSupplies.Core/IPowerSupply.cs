namespace PowerSupplies.Core;

public interface IPowerSupply
{
    string Info { get; }

    void Connect(string port);

    void Disconnect();

    void SetOutput(bool output);

    IEnumerable<IReadOnlyCurrentPoint> MeasureCurrent();
}