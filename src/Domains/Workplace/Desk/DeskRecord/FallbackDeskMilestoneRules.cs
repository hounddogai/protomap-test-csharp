namespace Acme.Workforce.Workplace.Desk.DeskRecord;

/// <summary>A rule a fallback desk milestone must pass. Check returns its problems, or none.</summary>
public interface IFallbackDeskMilestoneRule
{
    List<string> Check(FallbackDeskMilestone item);
}

public sealed record FallbackDeskMilestoneDurationMinutesRangeRule(int Min, int Max) : IFallbackDeskMilestoneRule
{
    public List<string> Check(FallbackDeskMilestone item)
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

public sealed record FallbackDeskMilestoneRequiredTitleRule(string Message) : IFallbackDeskMilestoneRule
{
    public List<string> Check(FallbackDeskMilestone item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record FallbackDeskMilestoneCeilingRangeRule(double Min, double Max) : IFallbackDeskMilestoneRule
{
    public List<string> Check(FallbackDeskMilestone item)
    {
        var valueValue = item.Ceiling;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"ceiling {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a fallback desk milestone.</summary>
public sealed record FallbackDeskMilestoneValidator(List<IFallbackDeskMilestoneRule> Rules)
{
    public List<string> Validate(FallbackDeskMilestone item)
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

    public void RequireValid(FallbackDeskMilestone item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<FallbackDeskMilestone> items)
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

/// <summary>Functions of fallback desk milestone rules.</summary>
public static class FallbackDeskMilestoneRules
{
    public static FallbackDeskMilestoneValidator DefaultFallbackDeskMilestoneValidator()
    {
        return new FallbackDeskMilestoneValidator(
            new List<IFallbackDeskMilestoneRule>
            {
                new FallbackDeskMilestoneDurationMinutesRangeRule(80, 3600),
                new FallbackDeskMilestoneRequiredTitleRule("fallback desk milestone needs a value"),
                new FallbackDeskMilestoneCeilingRangeRule(6.9, 211.2),
            });
    }
}
