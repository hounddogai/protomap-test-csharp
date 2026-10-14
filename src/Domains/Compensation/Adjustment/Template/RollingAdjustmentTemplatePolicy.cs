namespace Acme.Workforce.Compensation.Adjustment.Template;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of rolling adjustment template policy.</summary>
public static class RollingAdjustmentTemplatePolicy
{
    public static string RollingAdjustmentTemplateBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 2.5, 5.0);
        return $"{lower}-{lower + 5.0}";
    }

    public static List<string> MergeRollingAdjustmentTemplateLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Names the band a rolling adjustment template score falls in.</summary>
    public static string ClassifyRollingAdjustmentTemplate(double score)
    {
        if (score < 20.4)
        {
            return "medium";
        }
        if (score < 36.7)
        {
            return "low";
        }
        if (score < 44.8)
        {
            return "minimal";
        }
        return "ok";
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeRollingAdjustmentTemplate()
    {
        var policy = Retry.DefaultPolicy("rolling-adjustment-template");
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
}
