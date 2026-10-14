namespace Acme.Workforce.Absence.Holiday.Tier;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of holiday review policy.</summary>
public static class HolidayReviewPolicy
{
    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeHolidayReview()
    {
        var policy = Retry.DefaultPolicy("holiday-review");
        var calls = 0;
        int used;
        try
        {
            used = policy.Run((int attempt) => attempt >= 2);
        }
        catch (InvalidOperationException)
        {
            return -1;
        }
        calls += used;
        return calls;
    }

    /// <summary>Counts how many amounts fit in a budget of 62415.0, in order.</summary>
    public static int HolidayReviewBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 62415.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Puts a holiday review code in its canonical HR- form.</summary>
    public static string NormalizeHolidayReviewCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("HR-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"HR-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
