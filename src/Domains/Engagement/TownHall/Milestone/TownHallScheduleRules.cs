namespace Acme.Workforce.Engagement.TownHall.Milestone;

/// <summary>A rule a town hall schedule must pass. Check returns its problems, or none.</summary>
public interface ITownHallScheduleRule
{
    List<string> Check(TownHallSchedule item);
}

public sealed record TownHallScheduleRequiredSummaryRule(string Message) : ITownHallScheduleRule
{
    public List<string> Check(TownHallSchedule item)
    {
        if (item.Summary.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record TownHallScheduleHoursRangeRule(double Min, double Max) : ITownHallScheduleRule
{
    public List<string> Check(TownHallSchedule item)
    {
        var valueValue = item.Hours;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"hours {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record TownHallScheduleCodeLengthRule(int MaxLength) : ITownHallScheduleRule
{
    public List<string> Check(TownHallSchedule item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a town hall schedule.</summary>
public sealed record TownHallScheduleValidator(List<ITownHallScheduleRule> Rules)
{
    public List<string> Validate(TownHallSchedule item)
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

    public void RequireValid(TownHallSchedule item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<TownHallSchedule> items)
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

/// <summary>Functions of town hall schedule rules.</summary>
public static class TownHallScheduleRules
{
    public static TownHallScheduleValidator DefaultTownHallScheduleValidator()
    {
        return new TownHallScheduleValidator(
            new List<ITownHallScheduleRule>
            {
                new TownHallScheduleRequiredSummaryRule("town hall schedule needs an entry"),
                new TownHallScheduleHoursRangeRule(4.9, 780.2),
                new TownHallScheduleCodeLengthRule(32),
            });
    }
}
