namespace Acme.Workforce.Orgdesign.Span.Estimate;

/// <summary>A rule a primary span estimate must pass. Check returns its problems, or none.</summary>
public interface IPrimarySpanEstimateRule
{
    List<string> Check(PrimarySpanEstimate item);
}

public sealed record PrimarySpanEstimateCostCenterLengthRule(int MaxLength) : IPrimarySpanEstimateRule
{
    public List<string> Check(PrimarySpanEstimate item)
    {
        var size = item.CostCenter.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"cost center is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PrimarySpanEstimateCodePrefixRule(string Prefix) : IPrimarySpanEstimateRule
{
    public List<string> Check(PrimarySpanEstimate item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record PrimarySpanEstimateRetriesRangeRule(int Min, int Max) : IPrimarySpanEstimateRule
{
    public List<string> Check(PrimarySpanEstimate item)
    {
        var valueValue = item.Retries;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"retries {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"retries {valueValue} is above {Max}");
        }
        return problems;
    }
}

/// <summary>Runs every rule of a primary span estimate.</summary>
public sealed record PrimarySpanEstimateValidator(List<IPrimarySpanEstimateRule> Rules)
{
    public List<string> Validate(PrimarySpanEstimate item)
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

    public void RequireValid(PrimarySpanEstimate item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PrimarySpanEstimate> items)
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

/// <summary>Functions of primary span estimate rules.</summary>
public static class PrimarySpanEstimateRules
{
    public static PrimarySpanEstimateValidator DefaultPrimarySpanEstimateValidator()
    {
        return new PrimarySpanEstimateValidator(
            new List<IPrimarySpanEstimateRule>
            {
                new PrimarySpanEstimateCostCenterLengthRule(86400),
                new PrimarySpanEstimateCodePrefixRule("ACM"),
                new PrimarySpanEstimateRetriesRangeRule(64, 80),
            });
    }
}
