namespace PowerSupplies.Core;

public class Hmp2020 : HmpBase
{
    public Hmp2020() : base(2, 180.0, 80.0)
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

        SetVoltageCurrent(1, voltage1, current1);
        SetVoltageCurrent(2, voltage2, current2);
    }
}