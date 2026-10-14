namespace Acme.Workforce.Orgdesign.Span.Policy;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of span assignment policy.</summary>
public static class SpanAssignmentPolicy
{
    /// <summary>Counts how many amounts fit in a budget of 33113.0, in order.</summary>
    public static int SpanAssignmentBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 33113.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeSpanAssignment()
    {
        var policy = Retry.DefaultPolicy("span-assignment");
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

    /// <summary>Puts a span assignment code in its canonical SA- form.</summary>
    public static string NormalizeSpanAssignmentCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("SA-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"SA-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
