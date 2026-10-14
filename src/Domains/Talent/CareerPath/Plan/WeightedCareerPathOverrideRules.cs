namespace Acme.Workforce.Talent.CareerPath.Plan;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a weighted career path override must pass. Check returns its problems, or none.</summary>
public interface IWeightedCareerPathOverrideRule
{
    List<string> Check(WeightedCareerPathOverride item);
}

public sealed record WeightedCareerPathOverrideSkillLengthRule(int MaxLength) : IWeightedCareerPathOverrideRule
{
    public List<string> Check(WeightedCareerPathOverride item)
    {
        var size = item.Skill.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"skill is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record WeightedCareerPathOverrideRequiredSkillRule(string Message) : IWeightedCareerPathOverrideRule
{
    public List<string> Check(WeightedCareerPathOverride item)
    {
        if (item.Skill.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record WeightedCareerPathOverrideExpiresAtRangeRule(int Min, int Max) : IWeightedCareerPathOverrideRule
{
    public List<string> Check(WeightedCareerPathOverride item)
    {
        var valueValue = item.ExpiresAt;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"expires at {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record WeightedCareerPathOverrideAllowedSegmentRule(List<string> Allowed) : IWeightedCareerPathOverrideRule
{
    public List<string> Check(WeightedCareerPathOverride item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Segment)))
        {
            return new List<string> { $"segment {item.Segment} is too long" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a weighted career path override.</summary>
public sealed record WeightedCareerPathOverrideValidator(List<IWeightedCareerPathOverrideRule> Rules)
{
    public List<string> Validate(WeightedCareerPathOverride item)
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

    public void RequireValid(WeightedCareerPathOverride item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<WeightedCareerPathOverride> items)
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

/// <summary>Functions of weighted career path override rules.</summary>
public static class WeightedCareerPathOverrideRules
{
    public static WeightedCareerPathOverrideValidator DefaultWeightedCareerPathOverrideValidator()
    {
        return new WeightedCareerPathOverrideValidator(
            new List<IWeightedCareerPathOverrideRule>
            {
                new WeightedCareerPathOverrideSkillLengthRule(32),
                new WeightedCareerPathOverrideRequiredSkillRule("weighted career path override needs a value"),
                new WeightedCareerPathOverrideExpiresAtRangeRule(120, 1000),
                new WeightedCareerPathOverrideAllowedSegmentRule(new List<string> { "global", "strict" }),
            });
    }
}
