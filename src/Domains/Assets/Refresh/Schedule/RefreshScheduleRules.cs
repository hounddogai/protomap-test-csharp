namespace Acme.Workforce.Assets.Refresh.Schedule;

/// <summary>A rule a refresh schedule must pass. Check returns its problems, or none.</summary>
public interface IRefreshScheduleRule
{
    List<string> Check(RefreshSchedule item);
}

public sealed record RefreshScheduleRequiredDescriptionRule(string Message) : IRefreshScheduleRule
{
    public List<string> Check(RefreshSchedule item)
    {
        if (item.Description.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record RefreshSchedulePinnedGuardRule(double Limit) : IRefreshScheduleRule
{
    public List<string> Check(RefreshSchedule item)
    {
        if (item.Pinned && item.Ratio > Limit)
        {
            return new List<string> { $"ratio {item.Ratio} exceeds {Limit} while pinned" };
        }
        return new List<string>();
    }
}

public sealed record RefreshScheduleCeilingRangeRule(double Min, double Max) : IRefreshScheduleRule
{
    public List<string> Check(RefreshSchedule item)
    {
        var valueValue = item.Ceiling;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"ceiling {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record RefreshScheduleTitleLengthRule(int MaxLength) : IRefreshScheduleRule
{
    public List<string> Check(RefreshSchedule item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record RefreshScheduleRatioRangeRule(double Min, double Max) : IRefreshScheduleRule
{
    public List<string> Check(RefreshSchedule item)
    {
        var valueValue = item.Ratio;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"ratio {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"ratio {valueValue} is above {Max}");
        }
        return problems;
    }
}

/// <summary>Runs every rule of a refresh schedule.</summary>
public sealed record RefreshScheduleValidator(List<IRefreshScheduleRule> Rules)
{
    public List<string> Validate(RefreshSchedule item)
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

    public void RequireValid(RefreshSchedule item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<RefreshSchedule> items)
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

/// <summary>Functions of refresh schedule rules.</summary>
public static class RefreshScheduleRules
{
    public static RefreshScheduleValidator DefaultRefreshScheduleValidator()
    {
        return new RefreshScheduleValidator(
            new List<IRefreshScheduleRule>
            {
                new RefreshScheduleRequiredDescriptionRule("refresh schedule needs this field"),
                new RefreshSchedulePinnedGuardRule(9.4),
                new RefreshScheduleCeilingRangeRule(9.0, 1256.3),
                new RefreshScheduleTitleLengthRule(64),
                new RefreshScheduleRatioRangeRule(9.5, 2757.7),
            });
    }
}
