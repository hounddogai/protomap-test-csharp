namespace Acme.Workforce.Orgdesign.Scenario.Reservation;

/// <summary>A rule a scenario milestone must pass. Check returns its problems, or none.</summary>
public interface IScenarioMilestoneRule
{
    List<string> Check(ScenarioMilestone item);
}

public sealed record ScenarioMilestoneGraceDaysRangeRule(int Min, int Max) : IScenarioMilestoneRule
{
    public List<string> Check(ScenarioMilestone item)
    {
        var valueValue = item.GraceDays;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"grace days {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record ScenarioMilestoneTitleLengthRule(int MaxLength) : IScenarioMilestoneRule
{
    public List<string> Check(ScenarioMilestone item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ScenarioMilestoneBatchSizeRangeRule(int Min, int Max) : IScenarioMilestoneRule
{
    public List<string> Check(ScenarioMilestone item)
    {
        var valueValue = item.BatchSize;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"batch size {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a scenario milestone.</summary>
public sealed record ScenarioMilestoneValidator(List<IScenarioMilestoneRule> Rules)
{
    public List<string> Validate(ScenarioMilestone item)
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

    public void RequireValid(ScenarioMilestone item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ScenarioMilestone> items)
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

/// <summary>Functions of scenario milestone rules.</summary>
public static class ScenarioMilestoneRules
{
    public static ScenarioMilestoneValidator DefaultScenarioMilestoneValidator()
    {
        return new ScenarioMilestoneValidator(
            new List<IScenarioMilestoneRule>
            {
                new ScenarioMilestoneGraceDaysRangeRule(32, 80),
                new ScenarioMilestoneTitleLengthRule(64),
                new ScenarioMilestoneBatchSizeRangeRule(255, 86400),
            });
    }
}
