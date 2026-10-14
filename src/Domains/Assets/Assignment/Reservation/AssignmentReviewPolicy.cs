namespace Acme.Workforce.Assets.Assignment.Reservation;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of assignment review policy.</summary>
public static class AssignmentReviewPolicy
{
    /// <summary>Counts how many amounts fit in a budget of 54107.0, in order.</summary>
    public static int AssignmentReviewBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 54107.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    public static string AssignmentReviewBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 50.0, 100.0);
        return $"{lower}-{lower + 100.0}";
    }

    /// <summary>Puts a assignment review code in its canonical AR- form.</summary>
    public static string NormalizeAssignmentReviewCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("AR-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"AR-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Names the band a assignment review score falls in.</summary>
    public static string ClassifyAssignmentReview(double score)
    {
        if (score < 34.9)
        {
            return "elevated";
        }
        if (score < 46.5)
        {
            return "ok";
        }
        if (score < 78.5)
        {
            return "high";
        }
        if (score < 90.5)
        {
            return "minimal";
        }
        return "low";
    }
}
