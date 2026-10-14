namespace Acme.Workforce.Assets.AssetReturn.Template;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of asset return template policy.</summary>
public static class AssetReturnTemplatePolicy
{
    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeAssetReturnTemplate()
    {
        var policy = Retry.DefaultPolicy("asset-return-template");
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

    /// <summary>Names the band a asset return template score falls in.</summary>
    public static string ClassifyAssetReturnTemplate(double score)
    {
        if (score < 17.8)
        {
            return "elevated";
        }
        if (score < 38.5)
        {
            return "watch";
        }
        if (score < 64.7)
        {
            return "high";
        }
        if (score < 85.8)
        {
            return "minimal";
        }
        return "ok";
    }

    /// <summary>Counts how many amounts fit in a budget of 7069.0, in order.</summary>
    public static int AssetReturnTemplateBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 7069.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Puts a asset return template code in its canonical ART- form.</summary>
    public static string NormalizeAssetReturnTemplateCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("ART-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"ART-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
