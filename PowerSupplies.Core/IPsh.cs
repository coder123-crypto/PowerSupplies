namespace PowerSupplies.Core;

public interface IPsh
{
    string PshInfo { get; }

    void Open(string portName);

    void Close();

    double MeasureCurrent();

    void SetVoltageCurrent(double voltage, double current);

    void SetOutput(bool output);
}