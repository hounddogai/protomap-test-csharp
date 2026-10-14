namespace Acme.Workforce.Workplace.Occupancy.Segment;

/// <summary>A rule a occupancy milestone must pass. Check returns its problems, or none.</summary>
public interface IOccupancyMilestoneRule
{
    List<string> Check(OccupancyMilestone item);
}

public sealed record OccupancyMilestoneCodeLengthRule(int MaxLength) : IOccupancyMilestoneRule
{
    public List<string> Check(OccupancyMilestone item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record OccupancyMilestoneCreatedAtRangeRule(int Min, int Max) : IOccupancyMilestoneRule
{
    public List<string> Check(OccupancyMilestone item)
    {
        var valueValue = item.CreatedAt;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"created at {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record OccupancyMilestoneRequiredNameRule(string Message) : IOccupancyMilestoneRule
{
    public List<string> Check(OccupancyMilestone item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record OccupancyMilestoneFteRangeRule(double Min, double Max) : IOccupancyMilestoneRule
{
    public List<string> Check(OccupancyMilestone item)
    {
        var valueValue = item.Fte;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"fte {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"fte {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record OccupancyMilestoneCodePrefixRule(string Prefix) : IOccupancyMilestoneRule
{
    public List<string> Check(OccupancyMilestone item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record OccupancyMilestoneChecksumLengthRule(int MaxLength) : IOccupancyMilestoneRule
{
    public List<string> Check(OccupancyMilestone item)
    {
        var size = item.Checksum.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"checksum is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a occupancy milestone.</summary>
public sealed record OccupancyMilestoneValidator(List<IOccupancyMilestoneRule> Rules)
{
    public List<string> Validate(OccupancyMilestone item)
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

    public void RequireValid(OccupancyMilestone item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<OccupancyMilestone> items)
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

/// <summary>Functions of occupancy milestone rules.</summary>
public static class OccupancyMilestoneRules
{
    public static OccupancyMilestoneValidator DefaultOccupancyMilestoneValidator()
    {
        return new OccupancyMilestoneValidator(
            new List<IOccupancyMilestoneRule>
            {
                new OccupancyMilestoneCodeLengthRule(32),
                new OccupancyMilestoneCreatedAtRangeRule(16, 1000),
                new OccupancyMilestoneRequiredNameRule("occupancy milestone needs a value"),
                new OccupancyMilestoneFteRangeRule(6.2, 3309.3),
                new OccupancyMilestoneCodePrefixRule("Q"),
                new OccupancyMilestoneChecksumLengthRule(120),
            });
    }
}
