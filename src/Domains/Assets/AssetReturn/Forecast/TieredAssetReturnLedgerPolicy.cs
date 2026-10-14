namespace Acme.Workforce.Assets.AssetReturn.Forecast;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of tiered asset return ledger policy.</summary>
public static class TieredAssetReturnLedgerPolicy
{
    /// <summary>Names the band a tiered asset return ledger score falls in.</summary>
    public static string ClassifyTieredAssetReturnLedger(double score)
    {
        if (score < 5.0)
        {
            return "ok";
        }
        if (score < 46.8)
        {
            return "high";
        }
        return "minimal";
    }

    /// <summary>Puts a tiered asset return ledger code in its canonical TARL- form.</summary>
    public static string NormalizeTieredAssetReturnLedgerCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("TARL-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"TARL-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeTieredAssetReturnLedger()
    {
        var policy = Retry.DefaultPolicy("tiered-asset-return-ledger");
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
}
