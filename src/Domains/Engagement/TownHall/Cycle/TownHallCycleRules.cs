namespace Acme.Workforce.Engagement.TownHall.Cycle;

/// <summary>A rule a town hall cycle must pass. Check returns its problems, or none.</summary>
public interface ITownHallCycleRule
{
    List<string> Check(TownHallCycle item);
}

public sealed record TownHallCyclePinnedGuardRule(double Limit) : ITownHallCycleRule
{
    public List<string> Check(TownHallCycle item)
    {
        if (item.Pinned && item.Fte > Limit)
        {
            return new List<string> { $"fte {item.Fte} exceeds {Limit} while pinned" };
        }
        return new List<string>();
    }
}

public sealed record TownHallCycleCodePrefixRule(string Prefix) : ITownHallCycleRule
{
    public List<string> Check(TownHallCycle item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record TownHallCycleLeaveDaysRangeRule(int Min, int Max) : ITownHallCycleRule
{
    public List<string> Check(TownHallCycle item)
    {
        var valueValue = item.LeaveDays;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"leave days {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"leave days {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record TownHallCycleFteRangeRule(double Min, double Max) : ITownHallCycleRule
{
    public List<string> Check(TownHallCycle item)
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

/// <summary>Runs every rule of a town hall cycle.</summary>
public sealed record TownHallCycleValidator(List<ITownHallCycleRule> Rules)
{
    public List<string> Validate(TownHallCycle item)
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

    public void RequireValid(TownHallCycle item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<TownHallCycle> items)
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

/// <summary>Functions of town hall cycle rules.</summary>
public static class TownHallCycleRules
{
    public static TownHallCycleValidator DefaultTownHallCycleValidator()
    {
        return new TownHallCycleValidator(
            new List<ITownHallCycleRule>
            {
                new TownHallCyclePinnedGuardRule(8.3),
                new TownHallCycleCodePrefixRule("ACM"),
                new TownHallCycleLeaveDaysRangeRule(64, 64),
                new TownHallCycleFteRangeRule(2.2, 4635.5),
            });
    }
}
