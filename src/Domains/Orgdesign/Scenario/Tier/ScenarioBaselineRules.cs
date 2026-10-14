namespace Acme.Workforce.Orgdesign.Scenario.Tier;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a scenario baseline must pass. Check returns its problems, or none.</summary>
public interface IScenarioBaselineRule
{
    List<string> Check(ScenarioBaseline item);
}

public sealed record ScenarioBaselineVersionRangeRule(int Min, int Max) : IScenarioBaselineRule
{
    public List<string> Check(ScenarioBaseline item)
    {
        var valueValue = item.Version;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"version {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"version {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record ScenarioBaselineRevisionRangeRule(int Min, int Max) : IScenarioBaselineRule
{
    public List<string> Check(ScenarioBaseline item)
    {
        var valueValue = item.Revision;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"revision {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record ScenarioBaselineLabelLengthRule(int MaxLength) : IScenarioBaselineRule
{
    public List<string> Check(ScenarioBaseline item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ScenarioBaselineRequiredLabelRule(string Message) : IScenarioBaselineRule
{
    public List<string> Check(ScenarioBaseline item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ScenarioBaselineAllowedCodeRule(List<string> Allowed) : IScenarioBaselineRule
{
    public List<string> Check(ScenarioBaseline item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Code)))
        {
            return new List<string> { $"code {item.Code} is unknown" };
        }
        return new List<string>();
    }
}

public sealed record ScenarioBaselineCostRangeRule(double Min, double Max) : IScenarioBaselineRule
{
    public List<string> Check(ScenarioBaseline item)
    {
        var valueValue = item.Cost;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"cost {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"cost {valueValue} is above {Max}");
        }
        return problems;
    }
}

/// <summary>Runs every rule of a scenario baseline.</summary>
public sealed record ScenarioBaselineValidator(List<IScenarioBaselineRule> Rules)
{
    public List<string> Validate(ScenarioBaseline item)
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

    public void RequireValid(ScenarioBaseline item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ScenarioBaseline> items)
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

/// <summary>Functions of scenario baseline rules.</summary>
public static class ScenarioBaselineRules
{
    public static ScenarioBaselineValidator DefaultScenarioBaselineValidator()
    {
        return new ScenarioBaselineValidator(
            new List<IScenarioBaselineRule>
            {
                new ScenarioBaselineVersionRangeRule(64, 255),
                new ScenarioBaselineRevisionRangeRule(32, 120),
                new ScenarioBaselineLabelLengthRule(255),
                new ScenarioBaselineRequiredLabelRule("scenario baseline needs an entry"),
                new ScenarioBaselineAllowedCodeRule(new List<string> { "pending", "manual" }),
                new ScenarioBaselineCostRangeRule(3.5, 2366.6),
            });
    }
}
