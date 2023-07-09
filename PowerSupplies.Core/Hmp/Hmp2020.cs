namespace PowerSupplies.Core.Hmp;

public class Hmp2020 : HmpBase
{
    public Hmp2020() : base(new[] { 32.0, 32.0 }, new[] { 10.0, 5.0 }, new[] { 160.0, 80 })
    {
    }

    public void SetVoltageCurrent(double voltage1, double current1, double voltage2, double current2)
    {
        if (current1 > 10.0)
        {
            current1 = 10.0;
        }

        if (current2 > 5.0)
        {
            current2 = 5.0;
        }

        if (voltage2 > 6)
        {
            throw new Exception($"Вольтаж по 2 каналу {voltage2}, что скорее всего ненормально");
        }

        SetVoltageCurrent(voltage1, current1, 1);
        SetVoltageCurrent(voltage2, current2, 2);
    }
}