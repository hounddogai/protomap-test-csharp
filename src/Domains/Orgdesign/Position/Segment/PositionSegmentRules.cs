namespace Acme.Workforce.Orgdesign.Position.Segment;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a position segment must pass. Check returns its problems, or none.</summary>
public interface IPositionSegmentRule
{
    List<string> Check(PositionSegment item);
}

public sealed record PositionSegmentRatioRangeRule(double Min, double Max) : IPositionSegmentRule
{
    public List<string> Check(PositionSegment item)
    {
        var valueValue = item.Ratio;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"ratio {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record PositionSegmentCodeLengthRule(int MaxLength) : IPositionSegmentRule
{
    public List<string> Check(PositionSegment item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PositionSegmentExpiresAtRangeRule(int Min, int Max) : IPositionSegmentRule
{
    public List<string> Check(PositionSegment item)
    {
        var valueValue = item.ExpiresAt;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"expires at {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"expires at {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record PositionSegmentAllowedLabelRule(List<string> Allowed) : IPositionSegmentRule
{
    public List<string> Check(PositionSegment item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Label)))
        {
            return new List<string> { $"label {item.Label} is inconsistent" };
        }
        return new List<string>();
    }
}

public sealed record PositionSegmentSourceLengthRule(int MaxLength) : IPositionSegmentRule
{
    public List<string> Check(PositionSegment item)
    {
        var size = item.Source.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"source is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PositionSegmentChecksumLengthRule(int MaxLength) : IPositionSegmentRule
{
    public List<string> Check(PositionSegment item)
    {
        var size = item.Checksum.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"checksum is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a position segment.</summary>
public sealed record PositionSegmentValidator(List<IPositionSegmentRule> Rules)
{
    public List<string> Validate(PositionSegment item)
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

    public void RequireValid(PositionSegment item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PositionSegment> items)
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

/// <summary>Functions of position segment rules.</summary>
public static class PositionSegmentRules
{
    public static PositionSegmentValidator DefaultPositionSegmentValidator()
    {
        return new PositionSegmentValidator(
            new List<IPositionSegmentRule>
            {
                new PositionSegmentRatioRangeRule(2.6, 3885.5),
                new PositionSegmentCodeLengthRule(120),
                new PositionSegmentExpiresAtRangeRule(64, 120),
                new PositionSegmentAllowedLabelRule(new List<string> { "lenient" }),
                new PositionSegmentSourceLengthRule(1000),
                new PositionSegmentChecksumLengthRule(86400),
            });
    }
}
