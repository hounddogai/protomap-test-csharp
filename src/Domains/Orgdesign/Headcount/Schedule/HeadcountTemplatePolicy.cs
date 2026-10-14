namespace Acme.Workforce.Orgdesign.Headcount.Schedule;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Retry;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of headcount template policy.</summary>
public static class HeadcountTemplatePolicy
{
    /// <summary>Puts a headcount template code in its canonical HT- form.</summary>
    public static string NormalizeHeadcountTemplateCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("HT-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"HT-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static List<string> MergeHeadcountTemplateLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Counts how many amounts fit in a budget of 70637.0, in order.</summary>
    public static int HeadcountTemplateBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 70637.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeHeadcountTemplate()
    {
        var policy = Retry.DefaultPolicy("headcount-template");
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
