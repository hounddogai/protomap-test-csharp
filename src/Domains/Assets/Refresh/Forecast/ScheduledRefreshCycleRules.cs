namespace Acme.Workforce.Assets.Refresh.Forecast;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a scheduled refresh cycle must pass. Check returns its problems, or none.</summary>
public interface IScheduledRefreshCycleRule
{
    List<string> Check(ScheduledRefreshCycle item);
}

public sealed record ScheduledRefreshCycleDurationMinutesRangeRule(int Min, int Max) : IScheduledRefreshCycleRule
{
    public List<string> Check(ScheduledRefreshCycle item)
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

public sealed record ScheduledRefreshCycleRequiredCodeRule(string Message) : IScheduledRefreshCycleRule
{
    public List<string> Check(ScheduledRefreshCycle item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ScheduledRefreshCycleAllowedCodeRule(List<string> Allowed) : IScheduledRefreshCycleRule
{
    public List<string> Check(ScheduledRefreshCycle item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Code)))
        {
            return new List<string> { $"code {item.Code} is missing" };
        }
        return new List<string>();
    }
}

public sealed record ScheduledRefreshCycleChannelLengthRule(int MaxLength) : IScheduledRefreshCycleRule
{
    public List<string> Check(ScheduledRefreshCycle item)
    {
        var size = item.Channel.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"channel is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a scheduled refresh cycle.</summary>
public sealed record ScheduledRefreshCycleValidator(List<IScheduledRefreshCycleRule> Rules)
{
    public List<string> Validate(ScheduledRefreshCycle item)
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

    public void RequireValid(ScheduledRefreshCycle item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ScheduledRefreshCycle> items)
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

/// <summary>Functions of scheduled refresh cycle rules.</summary>
public static class ScheduledRefreshCycleRules
{
    public static ScheduledRefreshCycleValidator DefaultScheduledRefreshCycleValidator()
    {
        return new ScheduledRefreshCycleValidator(
            new List<IScheduledRefreshCycleRule>
            {
                new ScheduledRefreshCycleDurationMinutesRangeRule(120, 3600),
                new ScheduledRefreshCycleRequiredCodeRule("scheduled refresh cycle needs an entry"),
                new ScheduledRefreshCycleAllowedCodeRule(new List<string> { "strict" }),
                new ScheduledRefreshCycleChannelLengthRule(255),
            });
    }
}
