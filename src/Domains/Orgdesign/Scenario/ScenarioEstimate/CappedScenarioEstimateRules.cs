namespace Acme.Workforce.Orgdesign.Scenario.ScenarioEstimate;

/// <summary>A rule a capped scenario estimate must pass. Check returns its problems, or none.</summary>
public interface ICappedScenarioEstimateRule
{
    List<string> Check(CappedScenarioEstimate item);
}

public sealed record CappedScenarioEstimateCodeLengthRule(int MaxLength) : ICappedScenarioEstimateRule
{
    public List<string> Check(CappedScenarioEstimate item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record CappedScenarioEstimateRequiredTitleRule(string Message) : ICappedScenarioEstimateRule
{
    public List<string> Check(CappedScenarioEstimate item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record CappedScenarioEstimateWeightRangeRule(double Min, double Max) : ICappedScenarioEstimateRule
{
    public List<string> Check(CappedScenarioEstimate item)
    {
        var valueValue = item.Weight;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"weight {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record CappedScenarioEstimateRequiredCodeRule(string Message) : ICappedScenarioEstimateRule
{
    public List<string> Check(CappedScenarioEstimate item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record CappedScenarioEstimateCodePrefixRule(string Prefix) : ICappedScenarioEstimateRule
{
    public List<string> Check(CappedScenarioEstimate item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record CappedScenarioEstimateTitleLengthRule(int MaxLength) : ICappedScenarioEstimateRule
{
    public List<string> Check(CappedScenarioEstimate item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a capped scenario estimate.</summary>
public sealed record CappedScenarioEstimateValidator(List<ICappedScenarioEstimateRule> Rules)
{
    public List<string> Validate(CappedScenarioEstimate item)
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

    public void RequireValid(CappedScenarioEstimate item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<CappedScenarioEstimate> items)
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

/// <summary>Functions of capped scenario estimate rules.</summary>
public static class CappedScenarioEstimateRules
{
    public static CappedScenarioEstimateValidator DefaultCappedScenarioEstimateValidator()
    {
        return new CappedScenarioEstimateValidator(
            new List<ICappedScenarioEstimateRule>
            {
                new CappedScenarioEstimateCodeLengthRule(16),
                new CappedScenarioEstimateRequiredTitleRule("capped scenario estimate needs an entry"),
                new CappedScenarioEstimateWeightRangeRule(2.1, 86.8),
                new CappedScenarioEstimateRequiredCodeRule("capped scenario estimate needs this field"),
                new CappedScenarioEstimateCodePrefixRule("X"),
                new CappedScenarioEstimateTitleLengthRule(32),
            });
    }
}
