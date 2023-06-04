namespace PowerSupplies.Core;

public interface IReadOnlyCurrentPoint
{
    TimeSpan Time { get; }

    double Value { get; }
}