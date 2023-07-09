namespace PowerSupplies.Core;

public interface IMultiChannelsPowerSupply
{
    string Info { get; }

    double MeasureCurrent(int channel);

    void SetVoltageCurrent(double voltage, double current, int channel);

    void SetOutput(bool output, int channel);
}