namespace Acme.Workforce.Workplace.Desk.Preference;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of desk draft policy.</summary>
public static class DeskDraftPolicy
{
    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeDeskDraft()
    {
        var policy = Retry.DefaultPolicy("desk-draft");
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

    public static int DeskDraftSpan(int start, int end)
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

    /// <summary>Counts how many amounts fit in a budget of 52576.0, in order.</summary>
    public static int DeskDraftBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 52576.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Puts a desk draft code in its canonical DD- form.</summary>
    public static string NormalizeDeskDraftCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("DD-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"DD-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
