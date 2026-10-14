namespace Acme.Workforce.Orgdesign.Scenario.Draft;

/// <summary>A rule a scenario group must pass. Check returns its problems, or none.</summary>
public interface IScenarioGroupRule
{
    List<string> Check(ScenarioGroup item);
}

public sealed record ScenarioGroupTitleLengthRule(int MaxLength) : IScenarioGroupRule
{
    public List<string> Check(ScenarioGroup item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ScenarioGroupRequiredLabelRule(string Message) : IScenarioGroupRule
{
    public List<string> Check(ScenarioGroup item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ScenarioGroupRequiredPhoneRule(string Message) : IScenarioGroupRule
{
    public List<string> Check(ScenarioGroup item)
    {
        if (item.Phone.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ScenarioGroupCodePrefixRule(string Prefix) : IScenarioGroupRule
{
    public List<string> Check(ScenarioGroup item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record ScenarioGroupBudgetRangeRule(double Min, double Max) : IScenarioGroupRule
{
    public List<string> Check(ScenarioGroup item)
    {
        var valueValue = item.Budget;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"budget {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a scenario group.</summary>
public sealed record ScenarioGroupValidator(List<IScenarioGroupRule> Rules)
{
    public List<string> Validate(ScenarioGroup item)
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

    public void RequireValid(ScenarioGroup item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ScenarioGroup> items)
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

/// <summary>Functions of scenario group rules.</summary>
public static class ScenarioGroupRules
{
    public static ScenarioGroupValidator DefaultScenarioGroupValidator()
    {
        return new ScenarioGroupValidator(
            new List<IScenarioGroupRule>
            {
                new ScenarioGroupTitleLengthRule(64),
                new ScenarioGroupRequiredLabelRule("scenario group needs a value"),
                new ScenarioGroupRequiredPhoneRule("scenario group needs a value"),
                new ScenarioGroupCodePrefixRule("REF"),
                new ScenarioGroupBudgetRangeRule(7.9, 3774.5),
            });
    }
}
