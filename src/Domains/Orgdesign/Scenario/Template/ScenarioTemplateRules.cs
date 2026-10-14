namespace Acme.Workforce.Orgdesign.Scenario.Template;

/// <summary>A rule a scenario template must pass. Check returns its problems, or none.</summary>
public interface IScenarioTemplateRule
{
    List<string> Check(ScenarioTemplate item);
}

public sealed record ScenarioTemplateLabelLengthRule(int MaxLength) : IScenarioTemplateRule
{
    public List<string> Check(ScenarioTemplate item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ScenarioTemplateCodeLengthRule(int MaxLength) : IScenarioTemplateRule
{
    public List<string> Check(ScenarioTemplate item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ScenarioTemplateJobCodeLengthRule(int MaxLength) : IScenarioTemplateRule
{
    public List<string> Check(ScenarioTemplate item)
    {
        var size = item.JobCode.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"job code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ScenarioTemplateCeilingRangeRule(double Min, double Max) : IScenarioTemplateRule
{
    public List<string> Check(ScenarioTemplate item)
    {
        var valueValue = item.Ceiling;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"ceiling {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"ceiling {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record ScenarioTemplateLimitRangeRule(int Min, int Max) : IScenarioTemplateRule
{
    public List<string> Check(ScenarioTemplate item)
    {
        var valueValue = item.Limit;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"limit {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a scenario template.</summary>
public sealed record ScenarioTemplateValidator(List<IScenarioTemplateRule> Rules)
{
    public List<string> Validate(ScenarioTemplate item)
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

    public void RequireValid(ScenarioTemplate item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ScenarioTemplate> items)
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

/// <summary>Functions of scenario template rules.</summary>
public static class ScenarioTemplateRules
{
    public static ScenarioTemplateValidator DefaultScenarioTemplateValidator()
    {
        return new ScenarioTemplateValidator(
            new List<IScenarioTemplateRule>
            {
                new ScenarioTemplateLabelLengthRule(32),
                new ScenarioTemplateCodeLengthRule(255),
                new ScenarioTemplateJobCodeLengthRule(32),
                new ScenarioTemplateCeilingRangeRule(8.8, 791.8),
                new ScenarioTemplateLimitRangeRule(32, 86400),
            });
    }
}
