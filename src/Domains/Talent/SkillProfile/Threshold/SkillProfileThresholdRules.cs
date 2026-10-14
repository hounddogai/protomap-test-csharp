namespace Acme.Workforce.Talent.SkillProfile.Threshold;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a skill profile threshold must pass. Check returns its problems, or none.</summary>
public interface ISkillProfileThresholdRule
{
    List<string> Check(SkillProfileThreshold item);
}

public sealed record SkillProfileThresholdRequiredLabelRule(string Message) : ISkillProfileThresholdRule
{
    public List<string> Check(SkillProfileThreshold item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record SkillProfileThresholdRequiredGradeRule(string Message) : ISkillProfileThresholdRule
{
    public List<string> Check(SkillProfileThreshold item)
    {
        if (item.Grade.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record SkillProfileThresholdMinimumRangeRule(double Min, double Max) : ISkillProfileThresholdRule
{
    public List<string> Check(SkillProfileThreshold item)
    {
        var valueValue = item.Minimum;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"minimum {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record SkillProfileThresholdCapacityRangeRule(int Min, int Max) : ISkillProfileThresholdRule
{
    public List<string> Check(SkillProfileThreshold item)
    {
        var valueValue = item.Capacity;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"capacity {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"capacity {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record SkillProfileThresholdAllowedLabelRule(List<string> Allowed) : ISkillProfileThresholdRule
{
    public List<string> Check(SkillProfileThreshold item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Label)))
        {
            return new List<string> { $"label {item.Label} is duplicated" };
        }
        return new List<string>();
    }
}

public sealed record SkillProfileThresholdRequiredCodeRule(string Message) : ISkillProfileThresholdRule
{
    public List<string> Check(SkillProfileThreshold item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a skill profile threshold.</summary>
public sealed record SkillProfileThresholdValidator(List<ISkillProfileThresholdRule> Rules)
{
    public List<string> Validate(SkillProfileThreshold item)
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

    public void RequireValid(SkillProfileThreshold item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<SkillProfileThreshold> items)
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

/// <summary>Functions of skill profile threshold rules.</summary>
public static class SkillProfileThresholdRules
{
    public static SkillProfileThresholdValidator DefaultSkillProfileThresholdValidator()
    {
        return new SkillProfileThresholdValidator(
            new List<ISkillProfileThresholdRule>
            {
                new SkillProfileThresholdRequiredLabelRule("skill profile threshold needs this field"),
                new SkillProfileThresholdRequiredGradeRule("skill profile threshold needs a value"),
                new SkillProfileThresholdMinimumRangeRule(9.5, 795.1),
                new SkillProfileThresholdCapacityRangeRule(255, 3600),
                new SkillProfileThresholdAllowedLabelRule(new List<string> { "seasonal", "rolling", "rolling" }),
                new SkillProfileThresholdRequiredCodeRule("skill profile threshold needs a value"),
            });
    }
}
