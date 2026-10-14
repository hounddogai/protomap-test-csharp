namespace Acme.Workforce.Workplace.Occupancy.Estimate;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a occupancy tier must pass. Check returns its problems, or none.</summary>
public interface IOccupancyTierRule
{
    List<string> Check(OccupancyTier item);
}

public sealed record OccupancyTierAllowedBadgeRule(List<string> Allowed) : IOccupancyTierRule
{
    public List<string> Check(OccupancyTier item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Badge)))
        {
            return new List<string> { $"badge {item.Badge} is unknown" };
        }
        return new List<string>();
    }
}

public sealed record OccupancyTierRequiredCodeRule(string Message) : IOccupancyTierRule
{
    public List<string> Check(OccupancyTier item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record OccupancyTierRequiredTitleRule(string Message) : IOccupancyTierRule
{
    public List<string> Check(OccupancyTier item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record OccupancyTierRevisionRangeRule(int Min, int Max) : IOccupancyTierRule
{
    public List<string> Check(OccupancyTier item)
    {
        var valueValue = item.Revision;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"revision {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"revision {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record OccupancyTierCodePrefixRule(string Prefix) : IOccupancyTierRule
{
    public List<string> Check(OccupancyTier item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record OccupancyTierTitleLengthRule(int MaxLength) : IOccupancyTierRule
{
    public List<string> Check(OccupancyTier item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a occupancy tier.</summary>
public sealed record OccupancyTierValidator(List<IOccupancyTierRule> Rules)
{
    public List<string> Validate(OccupancyTier item)
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

    public void RequireValid(OccupancyTier item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<OccupancyTier> items)
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

/// <summary>Functions of occupancy tier rules.</summary>
public static class OccupancyTierRules
{
    public static OccupancyTierValidator DefaultOccupancyTierValidator()
    {
        return new OccupancyTierValidator(
            new List<IOccupancyTierRule>
            {
                new OccupancyTierAllowedBadgeRule(new List<string>()),
                new OccupancyTierRequiredCodeRule("occupancy tier needs this field"),
                new OccupancyTierRequiredTitleRule("occupancy tier needs an entry"),
                new OccupancyTierRevisionRangeRule(1000, 86400),
                new OccupancyTierCodePrefixRule("REF"),
                new OccupancyTierTitleLengthRule(64),
            });
    }
}
