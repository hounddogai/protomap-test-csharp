namespace Acme.Workforce.Talent.Nomination.Group;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of nomination assignment policy.</summary>
public static class NominationAssignmentPolicy
{
    public static double NominationAssignmentWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 10) * 2.89;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Puts a nomination assignment code in its canonical NA- form.</summary>
    public static string NormalizeNominationAssignmentCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("NA-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"NA-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Counts how many amounts fit in a budget of 36624.0, in order.</summary>
    public static int NominationAssignmentBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 36624.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Names the band a nomination assignment score falls in.</summary>
    public static string ClassifyNominationAssignment(double score)
    {
        if (score < 16.0)
        {
            return "elevated";
        }
        if (score < 19.3)
        {
            return "medium";
        }
        if (score < 75.5)
        {
            return "low";
        }
        if (score < 89.9)
        {
            return "minimal";
        }
        return "watch";
    }
}
