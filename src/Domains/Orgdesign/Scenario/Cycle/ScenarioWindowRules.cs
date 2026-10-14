namespace Acme.Workforce.Orgdesign.Scenario.Cycle;

/// <summary>A rule a scenario window must pass. Check returns its problems, or none.</summary>
public interface IScenarioWindowRule
{
    List<string> Check(ScenarioWindow item);
}

public sealed record ScenarioWindowMinimumRangeRule(double Min, double Max) : IScenarioWindowRule
{
    public List<string> Check(ScenarioWindow item)
    {
        var valueValue = item.Minimum;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"minimum {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"minimum {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record ScenarioWindowRequiredCodeRule(string Message) : IScenarioWindowRule
{
    public List<string> Check(ScenarioWindow item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ScenarioWindowNotesLengthRule(int MaxLength) : IScenarioWindowRule
{
    public List<string> Check(ScenarioWindow item)
    {
        var size = item.Notes.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"notes is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a scenario window.</summary>
public sealed record ScenarioWindowValidator(List<IScenarioWindowRule> Rules)
{
    public List<string> Validate(ScenarioWindow item)
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

    public void RequireValid(ScenarioWindow item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ScenarioWindow> items)
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

/// <summary>Functions of scenario window rules.</summary>
public static class ScenarioWindowRules
{
    public static ScenarioWindowValidator DefaultScenarioWindowValidator()
    {
        return new ScenarioWindowValidator(
            new List<IScenarioWindowRule>
            {
                new ScenarioWindowMinimumRangeRule(9.8, 785.1),
                new ScenarioWindowRequiredCodeRule("scenario window needs a value"),
                new ScenarioWindowNotesLengthRule(120),
            });
    }
}
