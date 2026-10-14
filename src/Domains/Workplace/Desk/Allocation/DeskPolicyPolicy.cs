namespace Acme.Workforce.Workplace.Desk.Allocation;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of desk policy policy.</summary>
public static class DeskPolicyPolicy
{
    public static int DeskPolicySpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 900)
        {
            return 0;
        }
        return span / 86400;
    }

    public static string DeskPolicyBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 5.0, 10.0);
        return $"{lower}-{lower + 10.0}";
    }
}
