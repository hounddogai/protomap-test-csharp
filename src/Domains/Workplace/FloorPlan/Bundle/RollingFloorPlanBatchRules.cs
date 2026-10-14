namespace Acme.Workforce.Workplace.FloorPlan.Bundle;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a rolling floor plan batch must pass. Check returns its problems, or none.</summary>
public interface IRollingFloorPlanBatchRule
{
    List<string> Check(RollingFloorPlanBatch item);
}

public sealed record RollingFloorPlanBatchTitleLengthRule(int MaxLength) : IRollingFloorPlanBatchRule
{
    public List<string> Check(RollingFloorPlanBatch item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record RollingFloorPlanBatchSkillLengthRule(int MaxLength) : IRollingFloorPlanBatchRule
{
    public List<string> Check(RollingFloorPlanBatch item)
    {
        var size = item.Skill.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"skill is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record RollingFloorPlanBatchRequiredTitleRule(string Message) : IRollingFloorPlanBatchRule
{
    public List<string> Check(RollingFloorPlanBatch item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record RollingFloorPlanBatchAllowedCodeRule(List<string> Allowed) : IRollingFloorPlanBatchRule
{
    public List<string> Check(RollingFloorPlanBatch item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Code)))
        {
            return new List<string> { $"code {item.Code} is out of range" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a rolling floor plan batch.</summary>
public sealed record RollingFloorPlanBatchValidator(List<IRollingFloorPlanBatchRule> Rules)
{
    public List<string> Validate(RollingFloorPlanBatch item)
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

    public void RequireValid(RollingFloorPlanBatch item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<RollingFloorPlanBatch> items)
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

/// <summary>Functions of rolling floor plan batch rules.</summary>
public static class RollingFloorPlanBatchRules
{
    public static RollingFloorPlanBatchValidator DefaultRollingFloorPlanBatchValidator()
    {
        return new RollingFloorPlanBatchValidator(
            new List<IRollingFloorPlanBatchRule>
            {
                new RollingFloorPlanBatchTitleLengthRule(3600),
                new RollingFloorPlanBatchSkillLengthRule(1000),
                new RollingFloorPlanBatchRequiredTitleRule("rolling floor plan batch needs this field"),
                new RollingFloorPlanBatchAllowedCodeRule(new List<string>()),
            });
    }
}
