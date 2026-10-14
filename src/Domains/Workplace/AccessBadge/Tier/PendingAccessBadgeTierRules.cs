namespace Acme.Workforce.Workplace.AccessBadge.Tier;

/// <summary>A rule a pending access badge tier must pass. Check returns its problems, or none.</summary>
public interface IPendingAccessBadgeTierRule
{
    List<string> Check(PendingAccessBadgeTier item);
}

public sealed record PendingAccessBadgeTierRequiredTitleRule(string Message) : IPendingAccessBadgeTierRule
{
    public List<string> Check(PendingAccessBadgeTier item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PendingAccessBadgeTierTitleLengthRule(int MaxLength) : IPendingAccessBadgeTierRule
{
    public List<string> Check(PendingAccessBadgeTier item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PendingAccessBadgeTierCodeLengthRule(int MaxLength) : IPendingAccessBadgeTierRule
{
    public List<string> Check(PendingAccessBadgeTier item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PendingAccessBadgeTierDurationMinutesRangeRule(int Min, int Max) : IPendingAccessBadgeTierRule
{
    public List<string> Check(PendingAccessBadgeTier item)
    {
        var valueValue = item.DurationMinutes;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"duration minutes {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record PendingAccessBadgeTierRequiredCodeRule(string Message) : IPendingAccessBadgeTierRule
{
    public List<string> Check(PendingAccessBadgeTier item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PendingAccessBadgeTierUpdatedAtRangeRule(int Min, int Max) : IPendingAccessBadgeTierRule
{
    public List<string> Check(PendingAccessBadgeTier item)
    {
        var valueValue = item.UpdatedAt;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"updated at {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"updated at {valueValue} is above {Max}");
        }
        return problems;
    }
}

/// <summary>Runs every rule of a pending access badge tier.</summary>
public sealed record PendingAccessBadgeTierValidator(List<IPendingAccessBadgeTierRule> Rules)
{
    public List<string> Validate(PendingAccessBadgeTier item)
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

    public void RequireValid(PendingAccessBadgeTier item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PendingAccessBadgeTier> items)
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

/// <summary>Functions of pending access badge tier rules.</summary>
public static class PendingAccessBadgeTierRules
{
    public static PendingAccessBadgeTierValidator DefaultPendingAccessBadgeTierValidator()
    {
        return new PendingAccessBadgeTierValidator(
            new List<IPendingAccessBadgeTierRule>
            {
                new PendingAccessBadgeTierRequiredTitleRule("pending access badge tier needs this field"),
                new PendingAccessBadgeTierTitleLengthRule(86400),
                new PendingAccessBadgeTierCodeLengthRule(32),
                new PendingAccessBadgeTierDurationMinutesRangeRule(16, 86400),
                new PendingAccessBadgeTierRequiredCodeRule("pending access badge tier needs this field"),
                new PendingAccessBadgeTierUpdatedAtRangeRule(16, 64),
            });
    }
}
