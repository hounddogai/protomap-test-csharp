namespace Acme.Workforce.Talent.SuccessionPlan.Score;

/// <summary>A rule a strict succession plan override must pass. Check returns its problems, or none.</summary>
public interface IStrictSuccessionPlanOverrideRule
{
    List<string> Check(StrictSuccessionPlanOverride item);
}

public sealed record StrictSuccessionPlanOverrideSkillLengthRule(int MaxLength) : IStrictSuccessionPlanOverrideRule
{
    public List<string> Check(StrictSuccessionPlanOverride item)
    {
        var size = item.Skill.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"skill is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record StrictSuccessionPlanOverrideRequiredSkillRule(string Message) : IStrictSuccessionPlanOverrideRule
{
    public List<string> Check(StrictSuccessionPlanOverride item)
    {
        if (item.Skill.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record StrictSuccessionPlanOverrideCodeLengthRule(int MaxLength) : IStrictSuccessionPlanOverrideRule
{
    public List<string> Check(StrictSuccessionPlanOverride item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record StrictSuccessionPlanOverrideFloorValueRangeRule(double Min, double Max) : IStrictSuccessionPlanOverrideRule
{
    public List<string> Check(StrictSuccessionPlanOverride item)
    {
        var valueValue = item.FloorValue;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"floor value {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a strict succession plan override.</summary>
public sealed record StrictSuccessionPlanOverrideValidator(List<IStrictSuccessionPlanOverrideRule> Rules)
{
    public List<string> Validate(StrictSuccessionPlanOverride item)
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

    public void RequireValid(StrictSuccessionPlanOverride item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<StrictSuccessionPlanOverride> items)
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

/// <summary>Functions of strict succession plan override rules.</summary>
public static class StrictSuccessionPlanOverrideRules
{
    public static StrictSuccessionPlanOverrideValidator DefaultStrictSuccessionPlanOverrideValidator()
    {
        return new StrictSuccessionPlanOverrideValidator(
            new List<IStrictSuccessionPlanOverrideRule>
            {
                new StrictSuccessionPlanOverrideSkillLengthRule(86400),
                new StrictSuccessionPlanOverrideRequiredSkillRule("strict succession plan override needs this field"),
                new StrictSuccessionPlanOverrideCodeLengthRule(16),
                new StrictSuccessionPlanOverrideFloorValueRangeRule(1.8, 597.4),
            });
    }
}
