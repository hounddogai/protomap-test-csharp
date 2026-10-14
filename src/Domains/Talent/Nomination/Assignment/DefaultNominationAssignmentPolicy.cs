namespace Acme.Workforce.Talent.Nomination.Assignment;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of default nomination assignment policy.</summary>
public static class DefaultNominationAssignmentPolicy
{
    /// <summary>Names the band a default nomination assignment score falls in.</summary>
    public static string ClassifyDefaultNominationAssignment(double score)
    {
        if (score < 28.8)
        {
            return "high";
        }
        if (score < 41.4)
        {
            return "ok";
        }
        if (score < 69.3)
        {
            return "watch";
        }
        return "low";
    }

    /// <summary>Puts a default nomination assignment code in its canonical DNA- form.</summary>
    public static string NormalizeDefaultNominationAssignmentCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("DNA-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"DNA-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeDefaultNominationAssignment()
    {
        var policy = Retry.DefaultPolicy("default-nomination-assignment");
        var calls = 0;
        int used;
        try
        {
            used = policy.Run((int attempt) => attempt >= 5);
        }
        catch (InvalidOperationException)
        {
            return -1;
        }
        calls += used;
        return calls;
    }

    /// <summary>Counts how many amounts fit in a budget of 3310.0, in order.</summary>
    public static int DefaultNominationAssignmentBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 3310.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}
