namespace Acme.Workforce.Orgdesign.Scenario.Estimate;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a scenario budget must pass. Check returns its problems, or none.</summary>
public interface IScenarioBudgetRule
{
    List<string> Check(ScenarioBudget item);
}

public sealed record ScenarioBudgetRequiredSegmentRule(string Message) : IScenarioBudgetRule
{
    public List<string> Check(ScenarioBudget item)
    {
        if (item.Segment.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ScenarioBudgetBadgeLengthRule(int MaxLength) : IScenarioBudgetRule
{
    public List<string> Check(ScenarioBudget item)
    {
        var size = item.Badge.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"badge is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ScenarioBudgetSegmentLengthRule(int MaxLength) : IScenarioBudgetRule
{
    public List<string> Check(ScenarioBudget item)
    {
        var size = item.Segment.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"segment is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ScenarioBudgetLabelLengthRule(int MaxLength) : IScenarioBudgetRule
{
    public List<string> Check(ScenarioBudget item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ScenarioBudgetAllowedLabelRule(List<string> Allowed) : IScenarioBudgetRule
{
    public List<string> Check(ScenarioBudget item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Label)))
        {
            return new List<string> { $"label {item.Label} is missing" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a scenario budget.</summary>
public sealed record ScenarioBudgetValidator(List<IScenarioBudgetRule> Rules)
{
    public List<string> Validate(ScenarioBudget item)
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

    public void RequireValid(ScenarioBudget item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ScenarioBudget> items)
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

/// <summary>Functions of scenario budget rules.</summary>
public static class ScenarioBudgetRules
{
    public static ScenarioBudgetValidator DefaultScenarioBudgetValidator()
    {
        return new ScenarioBudgetValidator(
            new List<IScenarioBudgetRule>
            {
                new ScenarioBudgetRequiredSegmentRule("scenario budget needs a value"),
                new ScenarioBudgetBadgeLengthRule(3600),
                new ScenarioBudgetSegmentLengthRule(120),
                new ScenarioBudgetLabelLengthRule(16),
                new ScenarioBudgetAllowedLabelRule(new List<string>()),
            });
    }
}
