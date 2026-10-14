namespace Acme.Workforce.Orgdesign.Headcount.Budget;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of batched headcount template policy.</summary>
public static class BatchedHeadcountTemplatePolicy
{
    /// <summary>Counts how many amounts fit in a budget of 7133.0, in order.</summary>
    public static int BatchedHeadcountTemplateBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 7133.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Names the band a batched headcount template score falls in.</summary>
    public static string ClassifyBatchedHeadcountTemplate(double score)
    {
        if (score < 46.3)
        {
            return "elevated";
        }
        if (score < 67.3)
        {
            return "medium";
        }
        return "minimal";
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeBatchedHeadcountTemplate()
    {
        var policy = Retry.DefaultPolicy("batched-headcount-template");
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

    /// <summary>Puts a batched headcount template code in its canonical BHT- form.</summary>
    public static string NormalizeBatchedHeadcountTemplateCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("BHT-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"BHT-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
