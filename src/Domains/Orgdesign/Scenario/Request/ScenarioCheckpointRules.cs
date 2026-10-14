namespace Acme.Workforce.Orgdesign.Scenario.Request;

/// <summary>A rule a scenario checkpoint must pass. Check returns its problems, or none.</summary>
public interface IScenarioCheckpointRule
{
    List<string> Check(ScenarioCheckpoint item);
}

public sealed record ScenarioCheckpointRequiredExternalRefRule(string Message) : IScenarioCheckpointRule
{
    public List<string> Check(ScenarioCheckpoint item)
    {
        if (item.ExternalRef.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ScenarioCheckpointTitleLengthRule(int MaxLength) : IScenarioCheckpointRule
{
    public List<string> Check(ScenarioCheckpoint item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ScenarioCheckpointRetriesRangeRule(int Min, int Max) : IScenarioCheckpointRule
{
    public List<string> Check(ScenarioCheckpoint item)
    {
        var valueValue = item.Retries;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"retries {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record ScenarioCheckpointDescriptionLengthRule(int MaxLength) : IScenarioCheckpointRule
{
    public List<string> Check(ScenarioCheckpoint item)
    {
        var size = item.Description.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"description is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a scenario checkpoint.</summary>
public sealed record ScenarioCheckpointValidator(List<IScenarioCheckpointRule> Rules)
{
    public List<string> Validate(ScenarioCheckpoint item)
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

    public void RequireValid(ScenarioCheckpoint item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ScenarioCheckpoint> items)
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

/// <summary>Functions of scenario checkpoint rules.</summary>
public static class ScenarioCheckpointRules
{
    public static ScenarioCheckpointValidator DefaultScenarioCheckpointValidator()
    {
        return new ScenarioCheckpointValidator(
            new List<IScenarioCheckpointRule>
            {
                new ScenarioCheckpointRequiredExternalRefRule("scenario checkpoint needs this field"),
                new ScenarioCheckpointTitleLengthRule(64),
                new ScenarioCheckpointRetriesRangeRule(32, 64),
                new ScenarioCheckpointDescriptionLengthRule(1000),
            });
    }
}
