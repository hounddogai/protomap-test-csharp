namespace Acme.Workforce.Orgdesign.Position.Template;

/// <summary>A rule a fallback position record must pass. Check returns its problems, or none.</summary>
public interface IFallbackPositionRecordRule
{
    List<string> Check(FallbackPositionRecord item);
}

public sealed record FallbackPositionRecordSkillLengthRule(int MaxLength) : IFallbackPositionRecordRule
{
    public List<string> Check(FallbackPositionRecord item)
    {
        var size = item.Skill.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"skill is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record FallbackPositionRecordCodePrefixRule(string Prefix) : IFallbackPositionRecordRule
{
    public List<string> Check(FallbackPositionRecord item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record FallbackPositionRecordFloorValueRangeRule(double Min, double Max) : IFallbackPositionRecordRule
{
    public List<string> Check(FallbackPositionRecord item)
    {
        var valueValue = item.FloorValue;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"floor value {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"floor value {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record FallbackPositionRecordCodeLengthRule(int MaxLength) : IFallbackPositionRecordRule
{
    public List<string> Check(FallbackPositionRecord item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a fallback position record.</summary>
public sealed record FallbackPositionRecordValidator(List<IFallbackPositionRecordRule> Rules)
{
    public List<string> Validate(FallbackPositionRecord item)
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

    public void RequireValid(FallbackPositionRecord item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<FallbackPositionRecord> items)
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

/// <summary>Functions of fallback position record rules.</summary>
public static class FallbackPositionRecordRules
{
    public static FallbackPositionRecordValidator DefaultFallbackPositionRecordValidator()
    {
        return new FallbackPositionRecordValidator(
            new List<IFallbackPositionRecordRule>
            {
                new FallbackPositionRecordSkillLengthRule(1000),
                new FallbackPositionRecordCodePrefixRule("OPS"),
                new FallbackPositionRecordFloorValueRangeRule(3.8, 4680.0),
                new FallbackPositionRecordCodeLengthRule(86400),
            });
    }
}
