namespace Acme.Workforce.Orgdesign.Scenario.Group;

/// <summary>A rule a weighted scenario group must pass. Check returns its problems, or none.</summary>
public interface IWeightedScenarioGroupRule
{
    List<string> Check(WeightedScenarioGroup item);
}

public sealed record WeightedScenarioGroupTitleLengthRule(int MaxLength) : IWeightedScenarioGroupRule
{
    public List<string> Check(WeightedScenarioGroup item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record WeightedScenarioGroupRequiredBadgeRule(string Message) : IWeightedScenarioGroupRule
{
    public List<string> Check(WeightedScenarioGroup item)
    {
        if (item.Badge.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record WeightedScenarioGroupRequiredCodeRule(string Message) : IWeightedScenarioGroupRule
{
    public List<string> Check(WeightedScenarioGroup item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record WeightedScenarioGroupBadgeLengthRule(int MaxLength) : IWeightedScenarioGroupRule
{
    public List<string> Check(WeightedScenarioGroup item)
    {
        var size = item.Badge.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"badge is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a weighted scenario group.</summary>
public sealed record WeightedScenarioGroupValidator(List<IWeightedScenarioGroupRule> Rules)
{
    public List<string> Validate(WeightedScenarioGroup item)
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

    public void RequireValid(WeightedScenarioGroup item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<WeightedScenarioGroup> items)
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

/// <summary>Functions of weighted scenario group rules.</summary>
public static class WeightedScenarioGroupRules
{
    public static WeightedScenarioGroupValidator DefaultWeightedScenarioGroupValidator()
    {
        return new WeightedScenarioGroupValidator(
            new List<IWeightedScenarioGroupRule>
            {
                new WeightedScenarioGroupTitleLengthRule(120),
                new WeightedScenarioGroupRequiredBadgeRule("weighted scenario group needs this field"),
                new WeightedScenarioGroupRequiredCodeRule("weighted scenario group needs a value"),
                new WeightedScenarioGroupBadgeLengthRule(64),
            });
    }
}
