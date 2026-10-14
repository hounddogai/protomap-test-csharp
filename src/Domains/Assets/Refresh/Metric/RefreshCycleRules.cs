namespace Acme.Workforce.Assets.Refresh.Metric;

/// <summary>A rule a refresh cycle must pass. Check returns its problems, or none.</summary>
public interface IRefreshCycleRule
{
    List<string> Check(RefreshCycle item);
}

public sealed record RefreshCycleNameLengthRule(int MaxLength) : IRefreshCycleRule
{
    public List<string> Check(RefreshCycle item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record RefreshCycleChannelLengthRule(int MaxLength) : IRefreshCycleRule
{
    public List<string> Check(RefreshCycle item)
    {
        var size = item.Channel.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"channel is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record RefreshCycleLeaveDaysRangeRule(int Min, int Max) : IRefreshCycleRule
{
    public List<string> Check(RefreshCycle item)
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

/// <summary>Runs every rule of a refresh cycle.</summary>
public sealed record RefreshCycleValidator(List<IRefreshCycleRule> Rules)
{
    public List<string> Validate(RefreshCycle item)
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

    public void RequireValid(RefreshCycle item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<RefreshCycle> items)
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

/// <summary>Functions of refresh cycle rules.</summary>
public static class RefreshCycleRules
{
    public static RefreshCycleValidator DefaultRefreshCycleValidator()
    {
        return new RefreshCycleValidator(
            new List<IRefreshCycleRule>
            {
                new RefreshCycleNameLengthRule(16),
                new RefreshCycleChannelLengthRule(3600),
                new RefreshCycleLeaveDaysRangeRule(255, 3600),
            });
    }
}
