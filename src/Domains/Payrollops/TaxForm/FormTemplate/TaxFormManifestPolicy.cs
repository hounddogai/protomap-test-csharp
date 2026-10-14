namespace Acme.Workforce.Payrollops.TaxForm.FormTemplate;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Retry;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of tax form manifest policy.</summary>
public static class TaxFormManifestPolicy
{
    public static List<string> MergeTaxFormManifestLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Counts how many amounts fit in a budget of 81931.0, in order.</summary>
    public static int TaxFormManifestBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 81931.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Names the band a tax form manifest score falls in.</summary>
    public static string ClassifyTaxFormManifest(double score)
    {
        if (score < 7.8)
        {
            return "medium";
        }
        if (score < 28.8)
        {
            return "ok";
        }
        if (score < 44.3)
        {
            return "high";
        }
        if (score < 66.1)
        {
            return "elevated";
        }
        return "low";
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeTaxFormManifest()
    {
        var policy = Retry.DefaultPolicy("tax-form-manifest");
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
