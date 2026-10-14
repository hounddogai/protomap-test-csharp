namespace Acme.Workforce.Orgdesign.Scenario.Policy;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a global scenario policy must pass. Check returns its problems, or none.</summary>
public interface IGlobalScenarioPolicyRule
{
    List<string> Check(GlobalScenarioPolicy item);
}

public sealed record GlobalScenarioPolicyFteRangeRule(double Min, double Max) : IGlobalScenarioPolicyRule
{
    public List<string> Check(GlobalScenarioPolicy item)
    {
        var valueValue = item.Fte;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"fte {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"fte {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record GlobalScenarioPolicyRequiredExternalRefRule(string Message) : IGlobalScenarioPolicyRule
{
    public List<string> Check(GlobalScenarioPolicy item)
    {
        if (item.ExternalRef.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record GlobalScenarioPolicyAllowedCodeRule(List<string> Allowed) : IGlobalScenarioPolicyRule
{
    public List<string> Check(GlobalScenarioPolicy item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Code)))
        {
            return new List<string> { $"code {item.Code} is above maximum" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a global scenario policy.</summary>
public sealed record GlobalScenarioPolicyValidator(List<IGlobalScenarioPolicyRule> Rules)
{
    public List<string> Validate(GlobalScenarioPolicy item)
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

    public void RequireValid(GlobalScenarioPolicy item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<GlobalScenarioPolicy> items)
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

/// <summary>Functions of global scenario policy rules.</summary>
public static class GlobalScenarioPolicyRules
{
    public static GlobalScenarioPolicyValidator DefaultGlobalScenarioPolicyValidator()
    {
        return new GlobalScenarioPolicyValidator(
            new List<IGlobalScenarioPolicyRule>
            {
                new GlobalScenarioPolicyFteRangeRule(3.2, 2831.1),
                new GlobalScenarioPolicyRequiredExternalRefRule("global scenario policy needs a value"),
                new GlobalScenarioPolicyAllowedCodeRule(new List<string>()),
            });
    }
}
