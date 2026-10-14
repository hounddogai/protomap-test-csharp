namespace Acme.Workforce.Contractors.Engagement.Profile;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of engagement profile policy.</summary>
public static class EngagementProfilePolicy
{
    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeEngagementProfile()
    {
        var policy = Retry.DefaultPolicy("engagement-profile");
        var calls = 0;
        int used;
        try
        {
            used = policy.Run((int attempt) => attempt >= 3);
        }
        catch (InvalidOperationException)
        {
            return -1;
        }
        calls += used;
        return calls;
    }

    /// <summary>Puts a engagement profile code in its canonical EP- form.</summary>
    public static string NormalizeEngagementProfileCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("EP-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"EP-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Counts how many amounts fit in a budget of 7525.0, in order.</summary>
    public static int EngagementProfileBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 7525.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Names the band a engagement profile score falls in.</summary>
    public static string ClassifyEngagementProfile(double score)
    {
        if (score < 15.5)
        {
            return "ok";
        }
        if (score < 49.2)
        {
            return "low";
        }
        if (score < 85.8)
        {
            return "minimal";
        }
        return "elevated";
    }
}
