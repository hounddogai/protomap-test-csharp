namespace Acme.Workforce.Orgdesign.Scenario.Audit;

/// <summary>A rule a scenario bundle must pass. Check returns its problems, or none.</summary>
public interface IScenarioBundleRule
{
    List<string> Check(ScenarioBundle item);
}

public sealed record ScenarioBundleRequiredNotesRule(string Message) : IScenarioBundleRule
{
    public List<string> Check(ScenarioBundle item)
    {
        if (item.Notes.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ScenarioBundleCountryLengthRule(int MaxLength) : IScenarioBundleRule
{
    public List<string> Check(ScenarioBundle item)
    {
        var size = item.Country.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"country is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ScenarioBundleNameLengthRule(int MaxLength) : IScenarioBundleRule
{
    public List<string> Check(ScenarioBundle item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a scenario bundle.</summary>
public sealed record ScenarioBundleValidator(List<IScenarioBundleRule> Rules)
{
    public List<string> Validate(ScenarioBundle item)
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

    public void RequireValid(ScenarioBundle item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ScenarioBundle> items)
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

/// <summary>Functions of scenario bundle rules.</summary>
public static class ScenarioBundleRules
{
    public static ScenarioBundleValidator DefaultScenarioBundleValidator()
    {
        return new ScenarioBundleValidator(
            new List<IScenarioBundleRule>
            {
                new ScenarioBundleRequiredNotesRule("scenario bundle needs this field"),
                new ScenarioBundleCountryLengthRule(255),
                new ScenarioBundleNameLengthRule(120),
            });
    }
}
