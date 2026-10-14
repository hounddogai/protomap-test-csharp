namespace Acme.Workforce.Orgdesign.Scenario.ScenarioRule;

/// <summary>A rule a scenario snapshot must pass. Check returns its problems, or none.</summary>
public interface IScenarioSnapshotRule
{
    List<string> Check(ScenarioSnapshot item);
}

public sealed record ScenarioSnapshotRequiredCodeRule(string Message) : IScenarioSnapshotRule
{
    public List<string> Check(ScenarioSnapshot item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ScenarioSnapshotCodePrefixRule(string Prefix) : IScenarioSnapshotRule
{
    public List<string> Check(ScenarioSnapshot item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record ScenarioSnapshotCodeLengthRule(int MaxLength) : IScenarioSnapshotRule
{
    public List<string> Check(ScenarioSnapshot item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ScenarioSnapshotMarginRangeRule(double Min, double Max) : IScenarioSnapshotRule
{
    public List<string> Check(ScenarioSnapshot item)
    {
        var valueValue = item.Margin;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"margin {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"margin {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record ScenarioSnapshotThresholdRangeRule(double Min, double Max) : IScenarioSnapshotRule
{
    public List<string> Check(ScenarioSnapshot item)
    {
        var valueValue = item.Threshold;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"threshold {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"threshold {valueValue} is above {Max}");
        }
        return problems;
    }
}

/// <summary>Runs every rule of a scenario snapshot.</summary>
public sealed record ScenarioSnapshotValidator(List<IScenarioSnapshotRule> Rules)
{
    public List<string> Validate(ScenarioSnapshot item)
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

    public void RequireValid(ScenarioSnapshot item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ScenarioSnapshot> items)
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

/// <summary>Functions of scenario snapshot rules.</summary>
public static class ScenarioSnapshotRules
{
    public static ScenarioSnapshotValidator DefaultScenarioSnapshotValidator()
    {
        return new ScenarioSnapshotValidator(
            new List<IScenarioSnapshotRule>
            {
                new ScenarioSnapshotRequiredCodeRule("scenario snapshot needs an entry"),
                new ScenarioSnapshotCodePrefixRule("REF"),
                new ScenarioSnapshotCodeLengthRule(1000),
                new ScenarioSnapshotMarginRangeRule(5.8, 2694.1),
                new ScenarioSnapshotThresholdRangeRule(3.8, 751.5),
            });
    }
}
