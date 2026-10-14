namespace Acme.Workforce.Orgdesign.Scenario.ScenarioTemplate;

/// <summary>A rule a scenario exception must pass. Check returns its problems, or none.</summary>
public interface IScenarioExceptionRule
{
    List<string> Check(ScenarioException item);
}

public sealed record ScenarioExceptionPhoneLengthRule(int MaxLength) : IScenarioExceptionRule
{
    public List<string> Check(ScenarioException item)
    {
        var size = item.Phone.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"phone is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ScenarioExceptionLabelLengthRule(int MaxLength) : IScenarioExceptionRule
{
    public List<string> Check(ScenarioException item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ScenarioExceptionRequiredPhoneRule(string Message) : IScenarioExceptionRule
{
    public List<string> Check(ScenarioException item)
    {
        if (item.Phone.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ScenarioExceptionRequiredLabelRule(string Message) : IScenarioExceptionRule
{
    public List<string> Check(ScenarioException item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a scenario exception.</summary>
public sealed record ScenarioExceptionValidator(List<IScenarioExceptionRule> Rules)
{
    public List<string> Validate(ScenarioException item)
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

    public void RequireValid(ScenarioException item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ScenarioException> items)
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

/// <summary>Functions of scenario exception rules.</summary>
public static class ScenarioExceptionRules
{
    public static ScenarioExceptionValidator DefaultScenarioExceptionValidator()
    {
        return new ScenarioExceptionValidator(
            new List<IScenarioExceptionRule>
            {
                new ScenarioExceptionPhoneLengthRule(80),
                new ScenarioExceptionLabelLengthRule(120),
                new ScenarioExceptionRequiredPhoneRule("scenario exception needs this field"),
                new ScenarioExceptionRequiredLabelRule("scenario exception needs an entry"),
            });
    }
}
