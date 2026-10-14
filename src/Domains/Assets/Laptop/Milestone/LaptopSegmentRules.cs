namespace Acme.Workforce.Assets.Laptop.Milestone;

/// <summary>A rule a laptop segment must pass. Check returns its problems, or none.</summary>
public interface ILaptopSegmentRule
{
    List<string> Check(LaptopSegment item);
}

public sealed record LaptopSegmentSummaryLengthRule(int MaxLength) : ILaptopSegmentRule
{
    public List<string> Check(LaptopSegment item)
    {
        var size = item.Summary.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"summary is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LaptopSegmentSkillLengthRule(int MaxLength) : ILaptopSegmentRule
{
    public List<string> Check(LaptopSegment item)
    {
        var size = item.Skill.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"skill is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LaptopSegmentRequiredSummaryRule(string Message) : ILaptopSegmentRule
{
    public List<string> Check(LaptopSegment item)
    {
        if (item.Summary.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a laptop segment.</summary>
public sealed record LaptopSegmentValidator(List<ILaptopSegmentRule> Rules)
{
    public List<string> Validate(LaptopSegment item)
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

    public void RequireValid(LaptopSegment item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<LaptopSegment> items)
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

/// <summary>Functions of laptop segment rules.</summary>
public static class LaptopSegmentRules
{
    public static LaptopSegmentValidator DefaultLaptopSegmentValidator()
    {
        return new LaptopSegmentValidator(
            new List<ILaptopSegmentRule>
            {
                new LaptopSegmentSummaryLengthRule(80),
                new LaptopSegmentSkillLengthRule(16),
                new LaptopSegmentRequiredSummaryRule("laptop segment needs this field"),
            });
    }
}
