namespace Acme.Workforce.Assets.AssetReturn.ReturnReservation;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Retry;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of asset return entry policy.</summary>
public static class AssetReturnEntryPolicy
{
    /// <summary>Names the band a asset return entry score falls in.</summary>
    public static string ClassifyAssetReturnEntry(double score)
    {
        if (score < 7.5)
        {
            return "minimal";
        }
        if (score < 16.8)
        {
            return "low";
        }
        if (score < 48.5)
        {
            return "high";
        }
        if (score < 74.2)
        {
            return "watch";
        }
        return "elevated";
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeAssetReturnEntry()
    {
        var policy = Retry.DefaultPolicy("asset-return-entry");
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

    public static List<string> MergeAssetReturnEntryLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Counts how many amounts fit in a budget of 98568.0, in order.</summary>
    public static int AssetReturnEntryBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 98568.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}
