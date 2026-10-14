namespace Acme.Workforce.Orgdesign.Scenario.ScenarioPolicy;

/// <summary>A rule a scenario agreement must pass. Check returns its problems, or none.</summary>
public interface IScenarioAgreementRule
{
    List<string> Check(ScenarioAgreement item);
}

public sealed record ScenarioAgreementRequiredCodeRule(string Message) : IScenarioAgreementRule
{
    public List<string> Check(ScenarioAgreement item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ScenarioAgreementNameLengthRule(int MaxLength) : IScenarioAgreementRule
{
    public List<string> Check(ScenarioAgreement item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ScenarioAgreementCodePrefixRule(string Prefix) : IScenarioAgreementRule
{
    public List<string> Check(ScenarioAgreement item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record ScenarioAgreementCodeLengthRule(int MaxLength) : IScenarioAgreementRule
{
    public List<string> Check(ScenarioAgreement item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a scenario agreement.</summary>
public sealed record ScenarioAgreementValidator(List<IScenarioAgreementRule> Rules)
{
    public List<string> Validate(ScenarioAgreement item)
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

    public void RequireValid(ScenarioAgreement item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ScenarioAgreement> items)
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

/// <summary>Functions of scenario agreement rules.</summary>
public static class ScenarioAgreementRules
{
    public static ScenarioAgreementValidator DefaultScenarioAgreementValidator()
    {
        return new ScenarioAgreementValidator(
            new List<IScenarioAgreementRule>
            {
                new ScenarioAgreementRequiredCodeRule("scenario agreement needs a value"),
                new ScenarioAgreementNameLengthRule(16),
                new ScenarioAgreementCodePrefixRule("ACM"),
                new ScenarioAgreementCodeLengthRule(3600),
            });
    }
}
