namespace PowerSupplies.Core;

public interface IHmp
{
    string HmpInfo { get; }

    void Open(string portName);

    void Close();

    double MeasureCurrent(int channel);

    void SetVoltageCurrent(double voltage, double current, int channel);

    void SetOutput(bool output, int channel);
}