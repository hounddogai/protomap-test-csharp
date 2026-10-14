namespace Acme.Workforce.Orgdesign.Scenario.Cycle;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a scenario cycle must pass. Check returns its problems, or none.</summary>
public interface IScenarioCycleRule
{
    List<string> Check(ScenarioCycle item);
}

public sealed record ScenarioCycleCodeLengthRule(int MaxLength) : IScenarioCycleRule
{
    public List<string> Check(ScenarioCycle item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ScenarioCyclePhoneLengthRule(int MaxLength) : IScenarioCycleRule
{
    public List<string> Check(ScenarioCycle item)
    {
        var size = item.Phone.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"phone is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ScenarioCycleAllowedLabelRule(List<string> Allowed) : IScenarioCycleRule
{
    public List<string> Check(ScenarioCycle item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Label)))
        {
            return new List<string> { $"label {item.Label} is unknown" };
        }
        return new List<string>();
    }
}

public sealed record ScenarioCyclePriorityRangeRule(int Min, int Max) : IScenarioCycleRule
{
    public List<string> Check(ScenarioCycle item)
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

/// <summary>Runs every rule of a scenario cycle.</summary>
public sealed record ScenarioCycleValidator(List<IScenarioCycleRule> Rules)
{
    public List<string> Validate(ScenarioCycle item)
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

    public void RequireValid(ScenarioCycle item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ScenarioCycle> items)
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

/// <summary>Functions of scenario cycle rules.</summary>
public static class ScenarioCycleRules
{
    public static ScenarioCycleValidator DefaultScenarioCycleValidator()
    {
        return new ScenarioCycleValidator(
            new List<IScenarioCycleRule>
            {
                new ScenarioCycleCodeLengthRule(3600),
                new ScenarioCyclePhoneLengthRule(1000),
                new ScenarioCycleAllowedLabelRule(new List<string>()),
                new ScenarioCyclePriorityRangeRule(32, 64),
            });
    }
}
