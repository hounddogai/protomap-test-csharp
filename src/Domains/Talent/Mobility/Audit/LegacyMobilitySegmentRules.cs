namespace Acme.Workforce.Talent.Mobility.Audit;

/// <summary>A rule a legacy mobility segment must pass. Check returns its problems, or none.</summary>
public interface ILegacyMobilitySegmentRule
{
    List<string> Check(LegacyMobilitySegment item);
}

public sealed record LegacyMobilitySegmentLimitRangeRule(int Min, int Max) : ILegacyMobilitySegmentRule
{
    public List<string> Check(LegacyMobilitySegment item)
    {
        var valueValue = item.Limit;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"limit {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"limit {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record LegacyMobilitySegmentGradeLengthRule(int MaxLength) : ILegacyMobilitySegmentRule
{
    public List<string> Check(LegacyMobilitySegment item)
    {
        var size = item.Grade.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"grade is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LegacyMobilitySegmentCeilingRangeRule(double Min, double Max) : ILegacyMobilitySegmentRule
{
    public List<string> Check(LegacyMobilitySegment item)
    {
        var valueValue = item.Ceiling;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"ceiling {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record LegacyMobilitySegmentRequiredCostCenterRule(string Message) : ILegacyMobilitySegmentRule
{
    public List<string> Check(LegacyMobilitySegment item)
    {
        if (item.CostCenter.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record LegacyMobilitySegmentRequiredCodeRule(string Message) : ILegacyMobilitySegmentRule
{
    public List<string> Check(LegacyMobilitySegment item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record LegacyMobilitySegmentDurationMinutesRangeRule(int Min, int Max) : ILegacyMobilitySegmentRule
{
    public List<string> Check(LegacyMobilitySegment item)
    {
        var valueValue = item.DurationMinutes;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"duration minutes {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"duration minutes {valueValue} is above {Max}");
        }
        return problems;
    }
}

/// <summary>Runs every rule of a legacy mobility segment.</summary>
public sealed record LegacyMobilitySegmentValidator(List<ILegacyMobilitySegmentRule> Rules)
{
    public List<string> Validate(LegacyMobilitySegment item)
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

    public void RequireValid(LegacyMobilitySegment item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<LegacyMobilitySegment> items)
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

/// <summary>Functions of legacy mobility segment rules.</summary>
public static class LegacyMobilitySegmentRules
{
    public static LegacyMobilitySegmentValidator DefaultLegacyMobilitySegmentValidator()
    {
        return new LegacyMobilitySegmentValidator(
            new List<ILegacyMobilitySegmentRule>
            {
                new LegacyMobilitySegmentLimitRangeRule(120, 255),
                new LegacyMobilitySegmentGradeLengthRule(255),
                new LegacyMobilitySegmentCeilingRangeRule(4.8, 3764.1),
                new LegacyMobilitySegmentRequiredCostCenterRule("legacy mobility segment needs this field"),
                new LegacyMobilitySegmentRequiredCodeRule("legacy mobility segment needs this field"),
                new LegacyMobilitySegmentDurationMinutesRangeRule(16, 32),
            });
    }
}
