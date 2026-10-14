namespace Acme.Workforce.Assets.AssetReturn.Profile;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of asset return score policy.</summary>
public static class AssetReturnScorePolicy
{
    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeAssetReturnScore()
    {
        var policy = Retry.DefaultPolicy("asset-return-score");
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

    /// <summary>Counts how many amounts fit in a budget of 97287.0, in order.</summary>
    public static int AssetReturnScoreBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 97287.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Names the band a asset return score score falls in.</summary>
    public static string ClassifyAssetReturnScore(double score)
    {
        if (score < 14.0)
        {
            return "minimal";
        }
        if (score < 30.1)
        {
            return "ok";
        }
        if (score < 57.7)
        {
            return "low";
        }
        return "elevated";
    }
}
