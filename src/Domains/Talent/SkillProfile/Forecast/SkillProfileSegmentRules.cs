namespace Acme.Workforce.Talent.SkillProfile.Forecast;

/// <summary>A rule a skill profile segment must pass. Check returns its problems, or none.</summary>
public interface ISkillProfileSegmentRule
{
    List<string> Check(SkillProfileSegment item);
}

public sealed record SkillProfileSegmentRequiredCodeRule(string Message) : ISkillProfileSegmentRule
{
    public List<string> Check(SkillProfileSegment item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record SkillProfileSegmentRequiredLabelRule(string Message) : ISkillProfileSegmentRule
{
    public List<string> Check(SkillProfileSegment item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record SkillProfileSegmentRequiredCostCenterRule(string Message) : ISkillProfileSegmentRule
{
    public List<string> Check(SkillProfileSegment item)
    {
        if (item.CostCenter.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record SkillProfileSegmentBatchSizeRangeRule(int Min, int Max) : ISkillProfileSegmentRule
{
    public List<string> Check(SkillProfileSegment item)
    {
        var valueValue = item.BatchSize;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"batch size {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"batch size {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record SkillProfileSegmentCodePrefixRule(string Prefix) : ISkillProfileSegmentRule
{
    public List<string> Check(SkillProfileSegment item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a skill profile segment.</summary>
public sealed record SkillProfileSegmentValidator(List<ISkillProfileSegmentRule> Rules)
{
    public List<string> Validate(SkillProfileSegment item)
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

    public void RequireValid(SkillProfileSegment item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<SkillProfileSegment> items)
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

/// <summary>Functions of skill profile segment rules.</summary>
public static class SkillProfileSegmentRules
{
    public static SkillProfileSegmentValidator DefaultSkillProfileSegmentValidator()
    {
        return new SkillProfileSegmentValidator(
            new List<ISkillProfileSegmentRule>
            {
                new SkillProfileSegmentRequiredCodeRule("skill profile segment needs an entry"),
                new SkillProfileSegmentRequiredLabelRule("skill profile segment needs an entry"),
                new SkillProfileSegmentRequiredCostCenterRule("skill profile segment needs this field"),
                new SkillProfileSegmentBatchSizeRangeRule(32, 3600),
                new SkillProfileSegmentCodePrefixRule("X"),
            });
    }
}
