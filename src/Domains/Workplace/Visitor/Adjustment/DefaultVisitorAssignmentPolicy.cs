namespace Acme.Workforce.Workplace.Visitor.Adjustment;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of default visitor assignment policy.</summary>
public static class DefaultVisitorAssignmentPolicy
{
    /// <summary>Puts a default visitor assignment code in its canonical DVA- form.</summary>
    public static string NormalizeDefaultVisitorAssignmentCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("DVA-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"DVA-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeDefaultVisitorAssignment()
    {
        var policy = Retry.DefaultPolicy("default-visitor-assignment");
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

    /// <summary>Counts how many amounts fit in a budget of 77688.0, in order.</summary>
    public static int DefaultVisitorAssignmentBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 77688.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Names the band a default visitor assignment score falls in.</summary>
    public static string ClassifyDefaultVisitorAssignment(double score)
    {
        if (score < 27.7)
        {
            return "medium";
        }
        if (score < 66.3)
        {
            return "low";
        }
        if (score < 99.1)
        {
            return "watch";
        }
        return "ok";
    }
}
