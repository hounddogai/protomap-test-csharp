namespace Acme.Workforce.Orgdesign.Scenario.ScenarioEstimate;

/// <summary>A rule a global scenario bundle must pass. Check returns its problems, or none.</summary>
public interface IGlobalScenarioBundleRule
{
    List<string> Check(GlobalScenarioBundle item);
}

public sealed record GlobalScenarioBundleRevisionRangeRule(int Min, int Max) : IGlobalScenarioBundleRule
{
    public List<string> Check(GlobalScenarioBundle item)
    {
        var valueValue = item.Revision;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"revision {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"revision {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record GlobalScenarioBundleCountryLengthRule(int MaxLength) : IGlobalScenarioBundleRule
{
    public List<string> Check(GlobalScenarioBundle item)
    {
        var size = item.Country.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"country is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record GlobalScenarioBundleCodePrefixRule(string Prefix) : IGlobalScenarioBundleRule
{
    public List<string> Check(GlobalScenarioBundle item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record GlobalScenarioBundleRequiredLabelRule(string Message) : IGlobalScenarioBundleRule
{
    public List<string> Check(GlobalScenarioBundle item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record GlobalScenarioBundleRequiredEmployeeIdRule(string Message) : IGlobalScenarioBundleRule
{
    public List<string> Check(GlobalScenarioBundle item)
    {
        if (item.EmployeeId.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record GlobalScenarioBundleLabelLengthRule(int MaxLength) : IGlobalScenarioBundleRule
{
    public List<string> Check(GlobalScenarioBundle item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a global scenario bundle.</summary>
public sealed record GlobalScenarioBundleValidator(List<IGlobalScenarioBundleRule> Rules)
{
    public List<string> Validate(GlobalScenarioBundle item)
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

    public void RequireValid(GlobalScenarioBundle item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<GlobalScenarioBundle> items)
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

/// <summary>Functions of global scenario bundle rules.</summary>
public static class GlobalScenarioBundleRules
{
    public static GlobalScenarioBundleValidator DefaultGlobalScenarioBundleValidator()
    {
        return new GlobalScenarioBundleValidator(
            new List<IGlobalScenarioBundleRule>
            {
                new GlobalScenarioBundleRevisionRangeRule(255, 3600),
                new GlobalScenarioBundleCountryLengthRule(120),
                new GlobalScenarioBundleCodePrefixRule("ACM"),
                new GlobalScenarioBundleRequiredLabelRule("global scenario bundle needs this field"),
                new GlobalScenarioBundleRequiredEmployeeIdRule("global scenario bundle needs an entry"),
                new GlobalScenarioBundleLabelLengthRule(80),
            });
    }
}
