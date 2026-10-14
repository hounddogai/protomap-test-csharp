namespace Acme.Workforce.Payrollops.TaxForm.FormGroup;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Retry;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of strict tax form group policy.</summary>
public static class StrictTaxFormGroupPolicy
{
    /// <summary>Counts how many amounts fit in a budget of 17849.0, in order.</summary>
    public static int StrictTaxFormGroupBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 17849.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeStrictTaxFormGroup()
    {
        var policy = Retry.DefaultPolicy("strict-tax-form-group");
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

    public static List<string> MergeStrictTaxFormGroupLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Names the band a strict tax form group score falls in.</summary>
    public static string ClassifyStrictTaxFormGroup(double score)
    {
        if (score < 24.9)
        {
            return "high";
        }
        if (score < 31.3)
        {
            return "elevated";
        }
        if (score < 78.2)
        {
            return "ok";
        }
        return "watch";
    }
}
