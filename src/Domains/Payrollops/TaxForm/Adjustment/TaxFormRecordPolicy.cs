namespace Acme.Workforce.Payrollops.TaxForm.Adjustment;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Retry;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of tax form record policy.</summary>
public static class TaxFormRecordPolicy
{
    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeTaxFormRecord()
    {
        var policy = Retry.DefaultPolicy("tax-form-record");
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

    /// <summary>Names the band a tax form record score falls in.</summary>
    public static string ClassifyTaxFormRecord(double score)
    {
        if (score < 13.5)
        {
            return "critical";
        }
        if (score < 59.3)
        {
            return "low";
        }
        if (score < 96.9)
        {
            return "medium";
        }
        return "high";
    }

    public static List<string> MergeTaxFormRecordLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }
}
