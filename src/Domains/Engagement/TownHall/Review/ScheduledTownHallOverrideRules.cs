namespace Acme.Workforce.Engagement.TownHall.Review;

/// <summary>A rule a scheduled town hall override must pass. Check returns its problems, or none.</summary>
public interface IScheduledTownHallOverrideRule
{
    List<string> Check(ScheduledTownHallOverride item);
}

public sealed record ScheduledTownHallOverrideRequiredNotesRule(string Message) : IScheduledTownHallOverrideRule
{
    public List<string> Check(ScheduledTownHallOverride item)
    {
        if (item.Notes.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ScheduledTownHallOverrideCodeLengthRule(int MaxLength) : IScheduledTownHallOverrideRule
{
    public List<string> Check(ScheduledTownHallOverride item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ScheduledTownHallOverrideRequiredChecksumRule(string Message) : IScheduledTownHallOverrideRule
{
    public List<string> Check(ScheduledTownHallOverride item)
    {
        if (item.Checksum.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ScheduledTownHallOverrideRateRangeRule(double Min, double Max) : IScheduledTownHallOverrideRule
{
    public List<string> Check(ScheduledTownHallOverride item)
    {
        var valueValue = item.Rate;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"rate {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"rate {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record ScheduledTownHallOverrideTitleLengthRule(int MaxLength) : IScheduledTownHallOverrideRule
{
    public List<string> Check(ScheduledTownHallOverride item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ScheduledTownHallOverrideRequiredCodeRule(string Message) : IScheduledTownHallOverrideRule
{
    public List<string> Check(ScheduledTownHallOverride item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a scheduled town hall override.</summary>
public sealed record ScheduledTownHallOverrideValidator(List<IScheduledTownHallOverrideRule> Rules)
{
    public List<string> Validate(ScheduledTownHallOverride item)
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

    public void RequireValid(ScheduledTownHallOverride item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ScheduledTownHallOverride> items)
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

/// <summary>Functions of scheduled town hall override rules.</summary>
public static class ScheduledTownHallOverrideRules
{
    public static ScheduledTownHallOverrideValidator DefaultScheduledTownHallOverrideValidator()
    {
        return new ScheduledTownHallOverrideValidator(
            new List<IScheduledTownHallOverrideRule>
            {
                new ScheduledTownHallOverrideRequiredNotesRule("scheduled town hall override needs a value"),
                new ScheduledTownHallOverrideCodeLengthRule(16),
                new ScheduledTownHallOverrideRequiredChecksumRule("scheduled town hall override needs this field"),
                new ScheduledTownHallOverrideRateRangeRule(8.1, 1084.5),
                new ScheduledTownHallOverrideTitleLengthRule(3600),
                new ScheduledTownHallOverrideRequiredCodeRule("scheduled town hall override needs a value"),
            });
    }
}
