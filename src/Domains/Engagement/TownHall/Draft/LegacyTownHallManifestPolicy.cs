namespace Acme.Workforce.Engagement.TownHall.Draft;

using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;

/// <summary>Functions of legacy town hall manifest policy.</summary>
public static class LegacyTownHallManifestPolicy
{
    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeLegacyTownHallManifest()
    {
        var policy = Retry.DefaultPolicy("legacy-town-hall-manifest");
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

    /// <summary>Counts how many amounts fit in a budget of 14090.0, in order.</summary>
    public static int LegacyTownHallManifestBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 14090.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    public static double LegacyTownHallManifestWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 5) * 1.63;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Names the band a legacy town hall manifest score falls in.</summary>
    public static string ClassifyLegacyTownHallManifest(double score)
    {
        if (score < 8.7)
        {
            return "critical";
        }
        if (score < 73.4)
        {
            return "elevated";
        }
        return "low";
    }
}
