namespace Acme.Workforce.Orgdesign.Scenario.Metric;

/// <summary>A rule a scenario metric must pass. Check returns its problems, or none.</summary>
public interface IScenarioMetricRule
{
    List<string> Check(ScenarioMetric item);
}

public sealed record ScenarioMetricExpiresAtRangeRule(int Min, int Max) : IScenarioMetricRule
{
    public List<string> Check(ScenarioMetric item)
    {
        var valueValue = item.ExpiresAt;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"expires at {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record ScenarioMetricRequiredCategoryRule(string Message) : IScenarioMetricRule
{
    public List<string> Check(ScenarioMetric item)
    {
        if (item.Category.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ScenarioMetricCategoryLengthRule(int MaxLength) : IScenarioMetricRule
{
    public List<string> Check(ScenarioMetric item)
    {
        var size = item.Category.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"category is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a scenario metric.</summary>
public sealed record ScenarioMetricValidator(List<IScenarioMetricRule> Rules)
{
    public List<string> Validate(ScenarioMetric item)
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

    public void RequireValid(ScenarioMetric item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ScenarioMetric> items)
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

/// <summary>Functions of scenario metric rules.</summary>
public static class ScenarioMetricRules
{
    public static ScenarioMetricValidator DefaultScenarioMetricValidator()
    {
        return new ScenarioMetricValidator(
            new List<IScenarioMetricRule>
            {
                new ScenarioMetricExpiresAtRangeRule(64, 255),
                new ScenarioMetricRequiredCategoryRule("scenario metric needs an entry"),
                new ScenarioMetricCategoryLengthRule(16),
            });
    }
}
