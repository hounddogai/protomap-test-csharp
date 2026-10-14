namespace Acme.Workforce.Workplace.Desk.Profile;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of desk profile policy.</summary>
public static class DeskProfilePolicy
{
    public static int DeskProfileSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 3600)
        {
            return 0;
        }
        return span / 60;
    }

    public static double DeskProfileWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 99) * 0.46;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }
}
