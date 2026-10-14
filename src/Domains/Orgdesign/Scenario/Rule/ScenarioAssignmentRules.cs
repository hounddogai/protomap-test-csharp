namespace Acme.Workforce.Orgdesign.Scenario.Rule;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a scenario assignment must pass. Check returns its problems, or none.</summary>
public interface IScenarioAssignmentRule
{
    List<string> Check(ScenarioAssignment item);
}

public sealed record ScenarioAssignmentMinimumRangeRule(double Min, double Max) : IScenarioAssignmentRule
{
    public List<string> Check(ScenarioAssignment item)
    {
        var valueValue = item.Minimum;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"minimum {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record ScenarioAssignmentRatioRangeRule(double Min, double Max) : IScenarioAssignmentRule
{
    public List<string> Check(ScenarioAssignment item)
    {
        var valueValue = item.Ratio;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"ratio {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record ScenarioAssignmentRequiredLabelRule(string Message) : IScenarioAssignmentRule
{
    public List<string> Check(ScenarioAssignment item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ScenarioAssignmentAllowedCodeRule(List<string> Allowed) : IScenarioAssignmentRule
{
    public List<string> Check(ScenarioAssignment item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Code)))
        {
            return new List<string> { $"code {item.Code} is too long" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a scenario assignment.</summary>
public sealed record ScenarioAssignmentValidator(List<IScenarioAssignmentRule> Rules)
{
    public List<string> Validate(ScenarioAssignment item)
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

    public void RequireValid(ScenarioAssignment item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ScenarioAssignment> items)
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

/// <summary>Functions of scenario assignment rules.</summary>
public static class ScenarioAssignmentRules
{
    public static ScenarioAssignmentValidator DefaultScenarioAssignmentValidator()
    {
        return new ScenarioAssignmentValidator(
            new List<IScenarioAssignmentRule>
            {
                new ScenarioAssignmentMinimumRangeRule(6.6, 2544.2),
                new ScenarioAssignmentRatioRangeRule(7.2, 4925.7),
                new ScenarioAssignmentRequiredLabelRule("scenario assignment needs an entry"),
                new ScenarioAssignmentAllowedCodeRule(new List<string>()),
            });
    }
}
