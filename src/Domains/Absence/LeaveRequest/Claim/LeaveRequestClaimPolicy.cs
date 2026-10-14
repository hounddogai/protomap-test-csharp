namespace Acme.Workforce.Absence.LeaveRequest.Claim;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of leave request claim policy.</summary>
public static class LeaveRequestClaimPolicy
{
    public static string LeaveRequestClaimBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 12.5, 25.0);
        return $"{lower}-{lower + 25.0}";
    }

    public static double LeaveRequestClaimWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 9) * 1.28;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Puts a leave request claim code in its canonical LRC- form.</summary>
    public static string NormalizeLeaveRequestClaimCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("LRC-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"LRC-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
