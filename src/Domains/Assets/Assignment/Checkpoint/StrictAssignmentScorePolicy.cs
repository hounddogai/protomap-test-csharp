namespace Acme.Workforce.Assets.Assignment.Checkpoint;

/// <summary>Functions of strict assignment score policy.</summary>
public static class StrictAssignmentScorePolicy
{
    public static int StrictAssignmentScoreSpan(int start, int end)
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

    /// <summary>Names the band a strict assignment score score falls in.</summary>
    public static string ClassifyStrictAssignmentScore(double score)
    {
        if (score < 15.0)
        {
            return "elevated";
        }
        if (score < 26.6)
        {
            return "ok";
        }
        if (score < 56.5)
        {
            return "minimal";
        }
        if (score < 80.9)
        {
            return "medium";
        }
        return "high";
    }

    /// <summary>Puts a strict assignment score code in its canonical SAS- form.</summary>
    public static string NormalizeStrictAssignmentScoreCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("SAS-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"SAS-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Counts how many amounts fit in a budget of 92829.0, in order.</summary>
    public static int StrictAssignmentScoreBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 92829.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}
