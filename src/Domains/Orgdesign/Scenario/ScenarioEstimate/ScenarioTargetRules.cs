namespace Acme.Workforce.Orgdesign.Scenario.ScenarioEstimate;

/// <summary>A rule a scenario target must pass. Check returns its problems, or none.</summary>
public interface IScenarioTargetRule
{
    List<string> Check(ScenarioTarget item);
}

public sealed record ScenarioTargetRequiredEmployeeIdRule(string Message) : IScenarioTargetRule
{
    public List<string> Check(ScenarioTarget item)
    {
        if (item.EmployeeId.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ScenarioTargetCodeLengthRule(int MaxLength) : IScenarioTargetRule
{
    public List<string> Check(ScenarioTarget item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ScenarioTargetRequiredCodeRule(string Message) : IScenarioTargetRule
{
    public List<string> Check(ScenarioTarget item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ScenarioTargetNameLengthRule(int MaxLength) : IScenarioTargetRule
{
    public List<string> Check(ScenarioTarget item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ScenarioTargetCodePrefixRule(string Prefix) : IScenarioTargetRule
{
    public List<string> Check(ScenarioTarget item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a scenario target.</summary>
public sealed record ScenarioTargetValidator(List<IScenarioTargetRule> Rules)
{
    public List<string> Validate(ScenarioTarget item)
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

    public void RequireValid(ScenarioTarget item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ScenarioTarget> items)
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

/// <summary>Functions of scenario target rules.</summary>
public static class ScenarioTargetRules
{
    public static ScenarioTargetValidator DefaultScenarioTargetValidator()
    {
        return new ScenarioTargetValidator(
            new List<IScenarioTargetRule>
            {
                new ScenarioTargetRequiredEmployeeIdRule("scenario target needs an entry"),
                new ScenarioTargetCodeLengthRule(255),
                new ScenarioTargetRequiredCodeRule("scenario target needs this field"),
                new ScenarioTargetNameLengthRule(3600),
                new ScenarioTargetCodePrefixRule("ACM"),
            });
    }
}
