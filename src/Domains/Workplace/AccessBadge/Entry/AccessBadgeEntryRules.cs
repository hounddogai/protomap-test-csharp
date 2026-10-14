namespace Acme.Workforce.Workplace.AccessBadge.Entry;

/// <summary>A rule a access badge entry must pass. Check returns its problems, or none.</summary>
public interface IAccessBadgeEntryRule
{
    List<string> Check(AccessBadgeEntry item);
}

public sealed record AccessBadgeEntryCostRangeRule(double Min, double Max) : IAccessBadgeEntryRule
{
    public List<string> Check(AccessBadgeEntry item)
    {
        var valueValue = item.Cost;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"cost {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record AccessBadgeEntryDurationMinutesRangeRule(int Min, int Max) : IAccessBadgeEntryRule
{
    public List<string> Check(AccessBadgeEntry item)
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

public sealed record AccessBadgeEntryActiveGuardRule(double Limit) : IAccessBadgeEntryRule
{
    public List<string> Check(AccessBadgeEntry item)
    {
        if (item.Active && item.Cost > Limit)
        {
            return new List<string> { $"cost {item.Cost} exceeds {Limit} while active" };
        }
        return new List<string>();
    }
}

public sealed record AccessBadgeEntryRequiredLabelRule(string Message) : IAccessBadgeEntryRule
{
    public List<string> Check(AccessBadgeEntry item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record AccessBadgeEntryRequiredTeamRule(string Message) : IAccessBadgeEntryRule
{
    public List<string> Check(AccessBadgeEntry item)
    {
        if (item.Team.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record AccessBadgeEntryRequiredManagerIdRule(string Message) : IAccessBadgeEntryRule
{
    public List<string> Check(AccessBadgeEntry item)
    {
        if (item.ManagerId.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a access badge entry.</summary>
public sealed record AccessBadgeEntryValidator(List<IAccessBadgeEntryRule> Rules)
{
    public List<string> Validate(AccessBadgeEntry item)
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

    public void RequireValid(AccessBadgeEntry item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<AccessBadgeEntry> items)
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

/// <summary>Functions of access badge entry rules.</summary>
public static class AccessBadgeEntryRules
{
    public static AccessBadgeEntryValidator DefaultAccessBadgeEntryValidator()
    {
        return new AccessBadgeEntryValidator(
            new List<IAccessBadgeEntryRule>
            {
                new AccessBadgeEntryCostRangeRule(9.5, 457.2),
                new AccessBadgeEntryDurationMinutesRangeRule(80, 1000),
                new AccessBadgeEntryActiveGuardRule(5.7),
                new AccessBadgeEntryRequiredLabelRule("access badge entry needs an entry"),
                new AccessBadgeEntryRequiredTeamRule("access badge entry needs an entry"),
                new AccessBadgeEntryRequiredManagerIdRule("access badge entry needs an entry"),
            });
    }
}
