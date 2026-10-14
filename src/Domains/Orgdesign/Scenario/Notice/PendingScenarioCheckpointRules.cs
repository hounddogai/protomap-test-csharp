namespace Acme.Workforce.Orgdesign.Scenario.Notice;

/// <summary>A rule a pending scenario checkpoint must pass. Check returns its problems, or none.</summary>
public interface IPendingScenarioCheckpointRule
{
    List<string> Check(PendingScenarioCheckpoint item);
}

public sealed record PendingScenarioCheckpointFteRangeRule(double Min, double Max) : IPendingScenarioCheckpointRule
{
    public List<string> Check(PendingScenarioCheckpoint item)
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

public sealed record PendingScenarioCheckpointFloorValueRangeRule(double Min, double Max) : IPendingScenarioCheckpointRule
{
    public List<string> Check(PendingScenarioCheckpoint item)
    {
        var valueValue = item.FloorValue;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"floor value {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"floor value {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record PendingScenarioCheckpointRequiredTeamRule(string Message) : IPendingScenarioCheckpointRule
{
    public List<string> Check(PendingScenarioCheckpoint item)
    {
        if (item.Team.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PendingScenarioCheckpointRequiredLabelRule(string Message) : IPendingScenarioCheckpointRule
{
    public List<string> Check(PendingScenarioCheckpoint item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a pending scenario checkpoint.</summary>
public sealed record PendingScenarioCheckpointValidator(List<IPendingScenarioCheckpointRule> Rules)
{
    public List<string> Validate(PendingScenarioCheckpoint item)
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

    public void RequireValid(PendingScenarioCheckpoint item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PendingScenarioCheckpoint> items)
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

/// <summary>Functions of pending scenario checkpoint rules.</summary>
public static class PendingScenarioCheckpointRules
{
    public static PendingScenarioCheckpointValidator DefaultPendingScenarioCheckpointValidator()
    {
        return new PendingScenarioCheckpointValidator(
            new List<IPendingScenarioCheckpointRule>
            {
                new PendingScenarioCheckpointFteRangeRule(4.9, 1617.1),
                new PendingScenarioCheckpointFloorValueRangeRule(1.5, 837.8),
                new PendingScenarioCheckpointRequiredTeamRule("pending scenario checkpoint needs a value"),
                new PendingScenarioCheckpointRequiredLabelRule("pending scenario checkpoint needs this field"),
            });
    }
}
