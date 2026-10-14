namespace Acme.Workforce.Engagement.TownHall.HallException;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a scheduled town hall exception must pass. Check returns its problems, or none.</summary>
public interface IScheduledTownHallExceptionRule
{
    List<string> Check(ScheduledTownHallException item);
}

public sealed record ScheduledTownHallExceptionAttemptsRangeRule(int Min, int Max) : IScheduledTownHallExceptionRule
{
    public List<string> Check(ScheduledTownHallException item)
    {
        var valueValue = item.Attempts;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"attempts {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"attempts {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record ScheduledTownHallExceptionRequiredNameRule(string Message) : IScheduledTownHallExceptionRule
{
    public List<string> Check(ScheduledTownHallException item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ScheduledTownHallExceptionAllowedNameRule(List<string> Allowed) : IScheduledTownHallExceptionRule
{
    public List<string> Check(ScheduledTownHallException item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Name)))
        {
            return new List<string> { $"name {item.Name} is below minimum" };
        }
        return new List<string>();
    }
}

public sealed record ScheduledTownHallExceptionRatingRangeRule(double Min, double Max) : IScheduledTownHallExceptionRule
{
    public List<string> Check(ScheduledTownHallException item)
    {
        var valueValue = item.Rating;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"rating {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"rating {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record ScheduledTownHallExceptionNameLengthRule(int MaxLength) : IScheduledTownHallExceptionRule
{
    public List<string> Check(ScheduledTownHallException item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ScheduledTownHallExceptionTitleLengthRule(int MaxLength) : IScheduledTownHallExceptionRule
{
    public List<string> Check(ScheduledTownHallException item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a scheduled town hall exception.</summary>
public sealed record ScheduledTownHallExceptionValidator(List<IScheduledTownHallExceptionRule> Rules)
{
    public List<string> Validate(ScheduledTownHallException item)
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

    public void RequireValid(ScheduledTownHallException item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ScheduledTownHallException> items)
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

/// <summary>Functions of scheduled town hall exception rules.</summary>
public static class ScheduledTownHallExceptionRules
{
    public static ScheduledTownHallExceptionValidator DefaultScheduledTownHallExceptionValidator()
    {
        return new ScheduledTownHallExceptionValidator(
            new List<IScheduledTownHallExceptionRule>
            {
                new ScheduledTownHallExceptionAttemptsRangeRule(64, 1000),
                new ScheduledTownHallExceptionRequiredNameRule("scheduled town hall exception needs an entry"),
                new ScheduledTownHallExceptionAllowedNameRule(new List<string>()),
                new ScheduledTownHallExceptionRatingRangeRule(2.6, 2794.0),
                new ScheduledTownHallExceptionNameLengthRule(86400),
                new ScheduledTownHallExceptionTitleLengthRule(120),
            });
    }
}
