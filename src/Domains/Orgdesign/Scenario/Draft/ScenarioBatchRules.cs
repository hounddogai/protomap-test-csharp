namespace Acme.Workforce.Orgdesign.Scenario.Draft;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a scenario batch must pass. Check returns its problems, or none.</summary>
public interface IScenarioBatchRule
{
    List<string> Check(ScenarioBatch item);
}

public sealed record ScenarioBatchCodeLengthRule(int MaxLength) : IScenarioBatchRule
{
    public List<string> Check(ScenarioBatch item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ScenarioBatchMaximumRangeRule(double Min, double Max) : IScenarioBatchRule
{
    public List<string> Check(ScenarioBatch item)
    {
        var valueValue = item.Maximum;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"maximum {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"maximum {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record ScenarioBatchAllowedTitleRule(List<string> Allowed) : IScenarioBatchRule
{
    public List<string> Check(ScenarioBatch item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Title)))
        {
            return new List<string> { $"title {item.Title} is above maximum" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a scenario batch.</summary>
public sealed record ScenarioBatchValidator(List<IScenarioBatchRule> Rules)
{
    public List<string> Validate(ScenarioBatch item)
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

    public void RequireValid(ScenarioBatch item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ScenarioBatch> items)
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

/// <summary>Functions of scenario batch rules.</summary>
public static class ScenarioBatchRules
{
    public static ScenarioBatchValidator DefaultScenarioBatchValidator()
    {
        return new ScenarioBatchValidator(
            new List<IScenarioBatchRule>
            {
                new ScenarioBatchCodeLengthRule(32),
                new ScenarioBatchMaximumRangeRule(8.7, 3317.4),
                new ScenarioBatchAllowedTitleRule(new List<string> { "seasonal", "global", "fallback" }),
            });
    }
}
