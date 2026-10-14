namespace Acme.Workforce.Orgdesign.Scenario.Request;

/// <summary>A rule a scenario schedule must pass. Check returns its problems, or none.</summary>
public interface IScenarioScheduleRule
{
    List<string> Check(ScenarioSchedule item);
}

public sealed record ScenarioSchedulePriorityRangeRule(int Min, int Max) : IScenarioScheduleRule
{
    public List<string> Check(ScenarioSchedule item)
    {
        var valueValue = item.Priority;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"priority {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"priority {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record ScenarioScheduleBatchSizeRangeRule(int Min, int Max) : IScenarioScheduleRule
{
    public List<string> Check(ScenarioSchedule item)
    {
        var valueValue = item.BatchSize;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"batch size {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record ScenarioScheduleCodePrefixRule(string Prefix) : IScenarioScheduleRule
{
    public List<string> Check(ScenarioSchedule item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record ScenarioScheduleRequiredCodeRule(string Message) : IScenarioScheduleRule
{
    public List<string> Check(ScenarioSchedule item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a scenario schedule.</summary>
public sealed record ScenarioScheduleValidator(List<IScenarioScheduleRule> Rules)
{
    public List<string> Validate(ScenarioSchedule item)
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

    public void RequireValid(ScenarioSchedule item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ScenarioSchedule> items)
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

/// <summary>Functions of scenario schedule rules.</summary>
public static class ScenarioScheduleRules
{
    public static ScenarioScheduleValidator DefaultScenarioScheduleValidator()
    {
        return new ScenarioScheduleValidator(
            new List<IScenarioScheduleRule>
            {
                new ScenarioSchedulePriorityRangeRule(64, 3600),
                new ScenarioScheduleBatchSizeRangeRule(32, 120),
                new ScenarioScheduleCodePrefixRule("REF"),
                new ScenarioScheduleRequiredCodeRule("scenario schedule needs this field"),
            });
    }
}
