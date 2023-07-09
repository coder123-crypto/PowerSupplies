namespace PowerSupplies.Core;

public interface IPowerSupplyListener
{
    IEnumerable<IReadOnlyCurrentPoint> MeasureCurrent();

    void ResetTimer();
}