namespace Acme.Workforce.Talent.Nomination.Assignment;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of lenient nomination request policy.</summary>
public static class LenientNominationRequestPolicy
{
    /// <summary>Puts a lenient nomination request code in its canonical LNR- form.</summary>
    public static string NormalizeLenientNominationRequestCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("LNR-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"LNR-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeLenientNominationRequest()
    {
        var policy = Retry.DefaultPolicy("lenient-nomination-request");
        var calls = 0;
        int used;
        try
        {
            used = policy.Run((int attempt) => attempt >= 1);
        }
        catch (InvalidOperationException)
        {
            return -1;
        }
        calls += used;
        return calls;
    }

    /// <summary>Counts how many amounts fit in a budget of 29834.0, in order.</summary>
    public static int LenientNominationRequestBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 29834.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}
