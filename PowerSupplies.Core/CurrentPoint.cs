namespace PowerSupplies.Core;

public record CurrentPoint(TimeSpan Time, double Value) : IReadOnlyCurrentPoint;