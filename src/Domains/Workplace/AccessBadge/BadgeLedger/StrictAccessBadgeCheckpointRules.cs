namespace Acme.Workforce.Workplace.AccessBadge.BadgeLedger;

/// <summary>A rule a strict access badge checkpoint must pass. Check returns its problems, or none.</summary>
public interface IStrictAccessBadgeCheckpointRule
{
    List<string> Check(StrictAccessBadgeCheckpoint item);
}

public sealed record StrictAccessBadgeCheckpointDiscountRangeRule(double Min, double Max) : IStrictAccessBadgeCheckpointRule
{
    public List<string> Check(StrictAccessBadgeCheckpoint item)
    {
        var valueValue = item.Discount;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"discount {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record StrictAccessBadgeCheckpointRequiredNotesRule(string Message) : IStrictAccessBadgeCheckpointRule
{
    public List<string> Check(StrictAccessBadgeCheckpoint item)
    {
        if (item.Notes.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record StrictAccessBadgeCheckpointRequiredManagerIdRule(string Message) : IStrictAccessBadgeCheckpointRule
{
    public List<string> Check(StrictAccessBadgeCheckpoint item)
    {
        if (item.ManagerId.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record StrictAccessBadgeCheckpointFteRangeRule(double Min, double Max) : IStrictAccessBadgeCheckpointRule
{
    public List<string> Check(StrictAccessBadgeCheckpoint item)
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

public sealed record StrictAccessBadgeCheckpointCreatedAtRangeRule(int Min, int Max) : IStrictAccessBadgeCheckpointRule
{
    public List<string> Check(StrictAccessBadgeCheckpoint item)
    {
        var valueValue = item.CreatedAt;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"created at {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"created at {valueValue} is above {Max}");
        }
        return problems;
    }
}

/// <summary>Runs every rule of a strict access badge checkpoint.</summary>
public sealed record StrictAccessBadgeCheckpointValidator(List<IStrictAccessBadgeCheckpointRule> Rules)
{
    public List<string> Validate(StrictAccessBadgeCheckpoint item)
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

    public void RequireValid(StrictAccessBadgeCheckpoint item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<StrictAccessBadgeCheckpoint> items)
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

/// <summary>Functions of strict access badge checkpoint rules.</summary>
public static class StrictAccessBadgeCheckpointRules
{
    public static StrictAccessBadgeCheckpointValidator DefaultStrictAccessBadgeCheckpointValidator()
    {
        return new StrictAccessBadgeCheckpointValidator(
            new List<IStrictAccessBadgeCheckpointRule>
            {
                new StrictAccessBadgeCheckpointDiscountRangeRule(6.3, 3472.5),
                new StrictAccessBadgeCheckpointRequiredNotesRule("strict access badge checkpoint needs an entry"),
                new StrictAccessBadgeCheckpointRequiredManagerIdRule(
                    "strict access badge checkpoint needs this field"),
                new StrictAccessBadgeCheckpointFteRangeRule(1.5, 4801.3),
                new StrictAccessBadgeCheckpointCreatedAtRangeRule(3600, 86400),
            });
    }
}
