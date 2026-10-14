namespace Acme.Workforce.Engagement.ActionPlan.Preference;

using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;

/// <summary>Functions of action plan assignment policy.</summary>
public static class ActionPlanAssignmentPolicy
{
    /// <summary>Puts a action plan assignment code in its canonical APA- form.</summary>
    public static string NormalizeActionPlanAssignmentCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("APA-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"APA-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeActionPlanAssignment()
    {
        var policy = Retry.DefaultPolicy("action-plan-assignment");
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

    /// <summary>Names the band a action plan assignment score falls in.</summary>
    public static string ClassifyActionPlanAssignment(double score)
    {
        if (score < 43.4)
        {
            return "medium";
        }
        if (score < 76.2)
        {
            return "ok";
        }
        return "watch";
    }

    public static string ActionPlanAssignmentBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 50.0, 100.0);
        return $"{lower}-{lower + 100.0}";
    }
}
