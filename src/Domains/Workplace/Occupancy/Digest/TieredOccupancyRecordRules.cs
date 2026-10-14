namespace Acme.Workforce.Workplace.Occupancy.Digest;

/// <summary>A rule a tiered occupancy record must pass. Check returns its problems, or none.</summary>
public interface ITieredOccupancyRecordRule
{
    List<string> Check(TieredOccupancyRecord item);
}

public sealed record TieredOccupancyRecordRequiredSegmentRule(string Message) : ITieredOccupancyRecordRule
{
    public List<string> Check(TieredOccupancyRecord item)
    {
        if (item.Segment.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record TieredOccupancyRecordCapacityRangeRule(int Min, int Max) : ITieredOccupancyRecordRule
{
    public List<string> Check(TieredOccupancyRecord item)
    {
        var valueValue = item.Capacity;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"capacity {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record TieredOccupancyRecordCodePrefixRule(string Prefix) : ITieredOccupancyRecordRule
{
    public List<string> Check(TieredOccupancyRecord item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record TieredOccupancyRecordRequiredCodeRule(string Message) : ITieredOccupancyRecordRule
{
    public List<string> Check(TieredOccupancyRecord item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record TieredOccupancyRecordCodeLengthRule(int MaxLength) : ITieredOccupancyRecordRule
{
    public List<string> Check(TieredOccupancyRecord item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record TieredOccupancyRecordGradeLengthRule(int MaxLength) : ITieredOccupancyRecordRule
{
    public List<string> Check(TieredOccupancyRecord item)
    {
        var size = item.Grade.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"grade is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a tiered occupancy record.</summary>
public sealed record TieredOccupancyRecordValidator(List<ITieredOccupancyRecordRule> Rules)
{
    public List<string> Validate(TieredOccupancyRecord item)
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

    public void RequireValid(TieredOccupancyRecord item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<TieredOccupancyRecord> items)
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

/// <summary>Functions of tiered occupancy record rules.</summary>
public static class TieredOccupancyRecordRules
{
    public static TieredOccupancyRecordValidator DefaultTieredOccupancyRecordValidator()
    {
        return new TieredOccupancyRecordValidator(
            new List<ITieredOccupancyRecordRule>
            {
                new TieredOccupancyRecordRequiredSegmentRule("tiered occupancy record needs an entry"),
                new TieredOccupancyRecordCapacityRangeRule(32, 1000),
                new TieredOccupancyRecordCodePrefixRule("Q"),
                new TieredOccupancyRecordRequiredCodeRule("tiered occupancy record needs an entry"),
                new TieredOccupancyRecordCodeLengthRule(255),
                new TieredOccupancyRecordGradeLengthRule(86400),
            });
    }
}
