namespace Acme.Workforce.Engagement.TownHall.Request;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of legacy town hall assignment policy.</summary>
public static class LegacyTownHallAssignmentPolicy
{
    public static int LegacyTownHallAssignmentSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 60)
        {
            return 0;
        }
        return span / 60;
    }

    /// <summary>Counts how many amounts fit in a budget of 14120.0, in order.</summary>
    public static int LegacyTownHallAssignmentBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 14120.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Puts a legacy town hall assignment code in its canonical LTHA- form.</summary>
    public static string NormalizeLegacyTownHallAssignmentCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("LTHA-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"LTHA-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeLegacyTownHallAssignment()
    {
        var policy = Retry.DefaultPolicy("legacy-town-hall-assignment");
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
}
