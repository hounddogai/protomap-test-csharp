namespace Acme.Workforce.Workplace.Visitor.Policy;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of visitor policy policy.</summary>
public static class VisitorPolicyPolicy
{
    public static int VisitorPolicySpan(int start, int end)
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
        return span / 3600;
    }

    public static string VisitorPolicyBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 12.5, 25.0);
        return $"{lower}-{lower + 25.0}";
    }
}
