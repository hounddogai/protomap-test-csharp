namespace Acme.Workforce.Absence.LeaveRequest.Reservation;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of leave request notice policy.</summary>
public static class LeaveRequestNoticePolicy
{
    /// <summary>Counts how many amounts fit in a budget of 52460.0, in order.</summary>
    public static int LeaveRequestNoticeBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 52460.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Puts a leave request notice code in its canonical LRN- form.</summary>
    public static string NormalizeLeaveRequestNoticeCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("LRN-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"LRN-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Names the band a leave request notice score falls in.</summary>
    public static string ClassifyLeaveRequestNotice(double score)
    {
        if (score < 1.9)
        {
            return "watch";
        }
        if (score < 4.3)
        {
            return "low";
        }
        if (score < 86.1)
        {
            return "elevated";
        }
        if (score < 88.9)
        {
            return "ok";
        }
        return "minimal";
    }

    public static double LeaveRequestNoticeWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 99) * 2.29;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }
}
