namespace Acme.Workforce.Workplace.Occupancy.Estimate;

/// <summary>A rule a regional occupancy tier must pass. Check returns its problems, or none.</summary>
public interface IRegionalOccupancyTierRule
{
    List<string> Check(RegionalOccupancyTier item);
}

public sealed record RegionalOccupancyTierRequiredNameRule(string Message) : IRegionalOccupancyTierRule
{
    public List<string> Check(RegionalOccupancyTier item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record RegionalOccupancyTierPinnedGuardRule(int Limit) : IRegionalOccupancyTierRule
{
    public List<string> Check(RegionalOccupancyTier item)
    {
        if (item.Pinned && item.LeaveDays > Limit)
        {
            return new List<string> { $"leave days {item.LeaveDays} exceeds {Limit} while pinned" };
        }
        return new List<string>();
    }
}

public sealed record RegionalOccupancyTierCodePrefixRule(string Prefix) : IRegionalOccupancyTierRule
{
    public List<string> Check(RegionalOccupancyTier item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a regional occupancy tier.</summary>
public sealed record RegionalOccupancyTierValidator(List<IRegionalOccupancyTierRule> Rules)
{
    public List<string> Validate(RegionalOccupancyTier item)
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

    public void RequireValid(RegionalOccupancyTier item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<RegionalOccupancyTier> items)
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

/// <summary>Functions of regional occupancy tier rules.</summary>
public static class RegionalOccupancyTierRules
{
    public static RegionalOccupancyTierValidator DefaultRegionalOccupancyTierValidator()
    {
        return new RegionalOccupancyTierValidator(
            new List<IRegionalOccupancyTierRule>
            {
                new RegionalOccupancyTierRequiredNameRule("regional occupancy tier needs a value"),
                new RegionalOccupancyTierPinnedGuardRule(120),
                new RegionalOccupancyTierCodePrefixRule("X"),
            });
    }
}
