namespace Acme.Workforce.Engagement.ActionPlan.Draft;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a scheduled action plan adjustment must pass. Check returns its problems, or none.</summary>
public interface IScheduledActionPlanAdjustmentRule
{
    List<string> Check(ScheduledActionPlanAdjustment item);
}

public sealed record ScheduledActionPlanAdjustmentDurationMinutesRangeRule(int Min, int Max) : IScheduledActionPlanAdjustmentRule
{
    public List<string> Check(ScheduledActionPlanAdjustment item)
    {
        var valueValue = item.DurationMinutes;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"duration minutes {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"duration minutes {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record ScheduledActionPlanAdjustmentAllowedCodeRule(List<string> Allowed) : IScheduledActionPlanAdjustmentRule
{
    public List<string> Check(ScheduledActionPlanAdjustment item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Code)))
        {
            return new List<string> { $"code {item.Code} is above maximum" };
        }
        return new List<string>();
    }
}

public sealed record ScheduledActionPlanAdjustmentCostRangeRule(double Min, double Max) : IScheduledActionPlanAdjustmentRule
{
    public List<string> Check(ScheduledActionPlanAdjustment item)
    {
        var valueValue = item.Cost;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"cost {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a scheduled action plan adjustment.</summary>
public sealed record ScheduledActionPlanAdjustmentValidator(List<IScheduledActionPlanAdjustmentRule> Rules)
{
    public List<string> Validate(ScheduledActionPlanAdjustment item)
    {
        var problems = new List<string>();
        foreach (var rule in Rules)
        {
            foreach (var problem in rule.Check(item))
            {
                problems.Add(problem);
            }
        }
        return problems;
    }

    public void RequireValid(ScheduledActionPlanAdjustment item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ScheduledActionPlanAdjustment> items)
    {
        var report = new Dictionary<string, List<string>>();
        foreach (var item in items)
        {
            var problems = Validate(item);
            if (problems.Count > 0)
            {
                report[item.Code] = problems;
            }
        }
        return report;
    }
}

/// <summary>Functions of scheduled action plan adjustment rules.</summary>
public static class ScheduledActionPlanAdjustmentRules
{
    public static ScheduledActionPlanAdjustmentValidator DefaultScheduledActionPlanAdjustmentValidator()
    {
        return new ScheduledActionPlanAdjustmentValidator(
            new List<IScheduledActionPlanAdjustmentRule>
            {
                new ScheduledActionPlanAdjustmentDurationMinutesRangeRule(80, 86400),
                new ScheduledActionPlanAdjustmentAllowedCodeRule(
                    new List<string> { "scheduled", "fallback", "manual" }),
                new ScheduledActionPlanAdjustmentCostRangeRule(0.7, 2465.0),
            });
    }
}
