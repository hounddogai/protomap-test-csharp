namespace Acme.Workforce.Contractors.Engagement.Tier;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a engagement plan must pass. Check returns its problems, or none.</summary>
public interface IEngagementPlanRule
{
    List<string> Check(EngagementPlan item);
}

public sealed record EngagementPlanSegmentLengthRule(int MaxLength) : IEngagementPlanRule
{
    public List<string> Check(EngagementPlan item)
    {
        var size = item.Segment.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"segment is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record EngagementPlanAllowedSegmentRule(List<string> Allowed) : IEngagementPlanRule
{
    public List<string> Check(EngagementPlan item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Segment)))
        {
            return new List<string> { $"segment {item.Segment} is stale" };
        }
        return new List<string>();
    }
}

public sealed record EngagementPlanCodeLengthRule(int MaxLength) : IEngagementPlanRule
{
    public List<string> Check(EngagementPlan item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record EngagementPlanQuantityRangeRule(int Min, int Max) : IEngagementPlanRule
{
    public List<string> Check(EngagementPlan item)
    {
        var valueValue = item.Quantity;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"quantity {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"quantity {valueValue} is above {Max}");
        }
        return problems;
    }
}

/// <summary>Runs every rule of a engagement plan.</summary>
public sealed record EngagementPlanValidator(List<IEngagementPlanRule> Rules)
{
    public List<string> Validate(EngagementPlan item)
    {
        var problems = new List<string>();
        foreach (var rule in Rules)
        {
            var found = rule.Check(item);
            if (found.Count == 0)
            {
                continue;
            }
            foreach (var problem in found)
            {
                problems.Add($"{item.Code}: {problem}");
            }
        }
        return problems;
    }

    public void RequireValid(EngagementPlan item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<EngagementPlan> items)
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

/// <summary>Functions of engagement plan rules.</summary>
public static class EngagementPlanRules
{
    public static EngagementPlanValidator DefaultEngagementPlanValidator()
    {
        return new EngagementPlanValidator(
            new List<IEngagementPlanRule>
            {
                new EngagementPlanSegmentLengthRule(80),
                new EngagementPlanAllowedSegmentRule(new List<string> { "default", "regional", "weighted" }),
                new EngagementPlanCodeLengthRule(80),
                new EngagementPlanQuantityRangeRule(16, 120),
            });
    }
}
