namespace Acme.Workforce.Payrollops.TaxForm.Group;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of tax form group policy.</summary>
public static class TaxFormGroupPolicy
{
    public static double TaxFormGroupWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 5) * 1.71;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    public static int TaxFormGroupSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 60)
        {
            return 0;
        }
        return span / 60;
    }
}
