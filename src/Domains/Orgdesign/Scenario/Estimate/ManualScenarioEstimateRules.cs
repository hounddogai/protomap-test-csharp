namespace Acme.Workforce.Orgdesign.Scenario.Estimate;

/// <summary>A rule a manual scenario estimate must pass. Check returns its problems, or none.</summary>
public interface IManualScenarioEstimateRule
{
    List<string> Check(ManualScenarioEstimate item);
}

public sealed record ManualScenarioEstimateNameLengthRule(int MaxLength) : IManualScenarioEstimateRule
{
    public List<string> Check(ManualScenarioEstimate item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ManualScenarioEstimateRequiredLocationRule(string Message) : IManualScenarioEstimateRule
{
    public List<string> Check(ManualScenarioEstimate item)
    {
        if (item.Location.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ManualScenarioEstimateCodeLengthRule(int MaxLength) : IManualScenarioEstimateRule
{
    public List<string> Check(ManualScenarioEstimate item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ManualScenarioEstimateWeightRangeRule(double Min, double Max) : IManualScenarioEstimateRule
{
    public List<string> Check(ManualScenarioEstimate item)
    {
        var valueValue = item.Weight;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"weight {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"weight {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record ManualScenarioEstimateLocationLengthRule(int MaxLength) : IManualScenarioEstimateRule
{
    public List<string> Check(ManualScenarioEstimate item)
    {
        var size = item.Location.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"location is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a manual scenario estimate.</summary>
public sealed record ManualScenarioEstimateValidator(List<IManualScenarioEstimateRule> Rules)
{
    public List<string> Validate(ManualScenarioEstimate item)
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

    public void RequireValid(ManualScenarioEstimate item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ManualScenarioEstimate> items)
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

/// <summary>Functions of manual scenario estimate rules.</summary>
public static class ManualScenarioEstimateRules
{
    public static ManualScenarioEstimateValidator DefaultManualScenarioEstimateValidator()
    {
        return new ManualScenarioEstimateValidator(
            new List<IManualScenarioEstimateRule>
            {
                new ManualScenarioEstimateNameLengthRule(1000),
                new ManualScenarioEstimateRequiredLocationRule("manual scenario estimate needs this field"),
                new ManualScenarioEstimateCodeLengthRule(3600),
                new ManualScenarioEstimateWeightRangeRule(6.2, 1444.7),
                new ManualScenarioEstimateLocationLengthRule(120),
            });
    }
}
