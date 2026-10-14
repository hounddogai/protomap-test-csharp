namespace Acme.Workforce.Workplace.Visitor.Draft;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of visitor manifest policy.</summary>
public static class VisitorManifestPolicy
{
    /// <summary>Counts how many amounts fit in a budget of 33328.0, in order.</summary>
    public static int VisitorManifestBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 33328.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Puts a visitor manifest code in its canonical VM- form.</summary>
    public static string NormalizeVisitorManifestCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("VM-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"VM-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeVisitorManifest()
    {
        var policy = Retry.DefaultPolicy("visitor-manifest");
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
}
