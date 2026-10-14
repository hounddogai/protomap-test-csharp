namespace Acme.Workforce.Orgdesign.Scenario.Rule;

/// <summary>A rule a scenario rule must pass. Check returns its problems, or none.</summary>
public interface IScenarioRuleRule
{
    List<string> Check(ScenarioRule item);
}

public sealed record ScenarioRuleEndsAtRangeRule(int Min, int Max) : IScenarioRuleRule
{
    public List<string> Check(ScenarioRule item)
    {
        var valueValue = item.EndsAt;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"ends at {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"ends at {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record ScenarioRuleRequiredCategoryRule(string Message) : IScenarioRuleRule
{
    public List<string> Check(ScenarioRule item)
    {
        if (item.Category.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ScenarioRuleRequiredReviewerRule(string Message) : IScenarioRuleRule
{
    public List<string> Check(ScenarioRule item)
    {
        if (item.Reviewer.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ScenarioRuleReviewerLengthRule(int MaxLength) : IScenarioRuleRule
{
    public List<string> Check(ScenarioRule item)
    {
        var size = item.Reviewer.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"reviewer is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a scenario rule.</summary>
public sealed record ScenarioRuleValidator(List<IScenarioRuleRule> Rules)
{
    public List<string> Validate(ScenarioRule item)
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

    public void RequireValid(ScenarioRule item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ScenarioRule> items)
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

/// <summary>Functions of scenario rule rules.</summary>
public static class ScenarioRuleRules
{
    public static ScenarioRuleValidator DefaultScenarioRuleValidator()
    {
        return new ScenarioRuleValidator(
            new List<IScenarioRuleRule>
            {
                new ScenarioRuleEndsAtRangeRule(32, 1000),
                new ScenarioRuleRequiredCategoryRule("scenario rule needs an entry"),
                new ScenarioRuleRequiredReviewerRule("scenario rule needs a value"),
                new ScenarioRuleReviewerLengthRule(86400),
            });
    }
}
