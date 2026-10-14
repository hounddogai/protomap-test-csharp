namespace Acme.Workforce.Orgdesign.Position.Template;

using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;

/// <summary>Functions of scheduled position template policy.</summary>
public static class ScheduledPositionTemplatePolicy
{
    public static int ScheduledPositionTemplateSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 300)
        {
            return 0;
        }
        return span / 3600;
    }

    public static string ScheduledPositionTemplateBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 25.0, 50.0);
        return $"{lower}-{lower + 50.0}";
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeScheduledPositionTemplate()
    {
        var policy = Retry.DefaultPolicy("scheduled-position-template");
        var calls = 0;
        int used;
        try
        {
            used = policy.Run((int attempt) => attempt >= 2);
        }
        catch (InvalidOperationException)
        {
            return -1;
        }
        calls += used;
        return calls;
    }

    /// <summary>Names the band a scheduled position template score falls in.</summary>
    public static string ClassifyScheduledPositionTemplate(double score)
    {
        if (score < 1.8)
        {
            return "medium";
        }
        if (score < 82.1)
        {
            return "ok";
        }
        return "critical";
    }
}
