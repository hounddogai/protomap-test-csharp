namespace Acme.Workforce.Talent.Pool.PoolAssignment;

/// <summary>A rule a legacy pool segment must pass. Check returns its problems, or none.</summary>
public interface ILegacyPoolSegmentRule
{
    List<string> Check(LegacyPoolSegment item);
}

public sealed record LegacyPoolSegmentExternalRefLengthRule(int MaxLength) : ILegacyPoolSegmentRule
{
    public List<string> Check(LegacyPoolSegment item)
    {
        var size = item.ExternalRef.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"external ref is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LegacyPoolSegmentCodePrefixRule(string Prefix) : ILegacyPoolSegmentRule
{
    public List<string> Check(LegacyPoolSegment item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record LegacyPoolSegmentCodeLengthRule(int MaxLength) : ILegacyPoolSegmentRule
{
    public List<string> Check(LegacyPoolSegment item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LegacyPoolSegmentRequiredLabelRule(string Message) : ILegacyPoolSegmentRule
{
    public List<string> Check(LegacyPoolSegment item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record LegacyPoolSegmentThresholdRangeRule(double Min, double Max) : ILegacyPoolSegmentRule
{
    public List<string> Check(LegacyPoolSegment item)
    {
        var valueValue = item.Threshold;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"threshold {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record LegacyPoolSegmentRequiredExternalRefRule(string Message) : ILegacyPoolSegmentRule
{
    public List<string> Check(LegacyPoolSegment item)
    {
        if (item.ExternalRef.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a legacy pool segment.</summary>
public sealed record LegacyPoolSegmentValidator(List<ILegacyPoolSegmentRule> Rules)
{
    public List<string> Validate(LegacyPoolSegment item)
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

    public void RequireValid(LegacyPoolSegment item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<LegacyPoolSegment> items)
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

/// <summary>Functions of legacy pool segment rules.</summary>
public static class LegacyPoolSegmentRules
{
    public static LegacyPoolSegmentValidator DefaultLegacyPoolSegmentValidator()
    {
        return new LegacyPoolSegmentValidator(
            new List<ILegacyPoolSegmentRule>
            {
                new LegacyPoolSegmentExternalRefLengthRule(255),
                new LegacyPoolSegmentCodePrefixRule("X"),
                new LegacyPoolSegmentCodeLengthRule(80),
                new LegacyPoolSegmentRequiredLabelRule("legacy pool segment needs this field"),
                new LegacyPoolSegmentThresholdRangeRule(5.8, 964.3),
                new LegacyPoolSegmentRequiredExternalRefRule("legacy pool segment needs a value"),
            });
    }
}
