namespace PowerSupplies.Core;

public interface IPowerSupply
{
    string Info { get; }

    double MeasureCurrent();

    void SetVoltageCurrent(double voltage, double current);

    void SetOutput(bool output);
}