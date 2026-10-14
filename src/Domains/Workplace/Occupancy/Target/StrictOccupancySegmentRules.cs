namespace Acme.Workforce.Workplace.Occupancy.Target;

/// <summary>A rule a strict occupancy segment must pass. Check returns its problems, or none.</summary>
public interface IStrictOccupancySegmentRule
{
    List<string> Check(StrictOccupancySegment item);
}

public sealed record StrictOccupancySegmentRequiredLabelRule(string Message) : IStrictOccupancySegmentRule
{
    public List<string> Check(StrictOccupancySegment item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record StrictOccupancySegmentCodePrefixRule(string Prefix) : IStrictOccupancySegmentRule
{
    public List<string> Check(StrictOccupancySegment item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record StrictOccupancySegmentRequiredRegionRule(string Message) : IStrictOccupancySegmentRule
{
    public List<string> Check(StrictOccupancySegment item)
    {
        if (item.Region.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record StrictOccupancySegmentRegionLengthRule(int MaxLength) : IStrictOccupancySegmentRule
{
    public List<string> Check(StrictOccupancySegment item)
    {
        var size = item.Region.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"region is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record StrictOccupancySegmentManagerIdLengthRule(int MaxLength) : IStrictOccupancySegmentRule
{
    public List<string> Check(StrictOccupancySegment item)
    {
        var size = item.ManagerId.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"manager id is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a strict occupancy segment.</summary>
public sealed record StrictOccupancySegmentValidator(List<IStrictOccupancySegmentRule> Rules)
{
    public List<string> Validate(StrictOccupancySegment item)
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

    public void RequireValid(StrictOccupancySegment item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<StrictOccupancySegment> items)
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

/// <summary>Functions of strict occupancy segment rules.</summary>
public static class StrictOccupancySegmentRules
{
    public static StrictOccupancySegmentValidator DefaultStrictOccupancySegmentValidator()
    {
        return new StrictOccupancySegmentValidator(
            new List<IStrictOccupancySegmentRule>
            {
                new StrictOccupancySegmentRequiredLabelRule("strict occupancy segment needs an entry"),
                new StrictOccupancySegmentCodePrefixRule("ACM"),
                new StrictOccupancySegmentRequiredRegionRule("strict occupancy segment needs an entry"),
                new StrictOccupancySegmentRegionLengthRule(3600),
                new StrictOccupancySegmentManagerIdLengthRule(32),
            });
    }
}
