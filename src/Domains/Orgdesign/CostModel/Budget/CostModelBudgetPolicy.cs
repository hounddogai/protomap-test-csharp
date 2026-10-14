namespace Acme.Workforce.Orgdesign.CostModel.Budget;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Retry;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of cost model budget policy.</summary>
public static class CostModelBudgetPolicy
{
    /// <summary>Puts a cost model budget code in its canonical CMB- form.</summary>
    public static string NormalizeCostModelBudgetCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("CMB-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"CMB-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeCostModelBudget()
    {
        var policy = Retry.DefaultPolicy("cost-model-budget");
        var calls = 0;
        int used;
        try
        {
            used = policy.Run((int attempt) => attempt >= 4);
        }
        catch (InvalidOperationException)
        {
            return -1;
        }
        calls += used;
        return calls;
    }

    public static int CostModelBudgetSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 60)
        {
            return 0;
        }
        return span / 3600;
    }

    public static List<string> MergeCostModelBudgetLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }
}
