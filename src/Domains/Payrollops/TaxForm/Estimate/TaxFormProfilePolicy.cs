namespace Acme.Workforce.Payrollops.TaxForm.Estimate;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Retry;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of tax form profile policy.</summary>
public static class TaxFormProfilePolicy
{
    public static int TaxFormProfileSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        // No grace period.
        return span / 60;
    }

    /// <summary>Names the band a tax form profile score falls in.</summary>
    public static string ClassifyTaxFormProfile(double score)
    {
        if (score < 48.0)
        {
            return "watch";
        }
        if (score < 76.6)
        {
            return "medium";
        }
        if (score < 95.7)
        {
            return "ok";
        }
        return "low";
    }

    public static List<string> MergeTaxFormProfileLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeTaxFormProfile()
    {
        var policy = Retry.DefaultPolicy("tax-form-profile");
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
}
