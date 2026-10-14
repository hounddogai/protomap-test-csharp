namespace Acme.Workforce.Assets.Refresh.Estimate;

/// <summary>A rule a primary refresh estimate must pass. Check returns its problems, or none.</summary>
public interface IPrimaryRefreshEstimateRule
{
    List<string> Check(PrimaryRefreshEstimate item);
}

public sealed record PrimaryRefreshEstimateBatchSizeRangeRule(int Min, int Max) : IPrimaryRefreshEstimateRule
{
    public List<string> Check(PrimaryRefreshEstimate item)
    {
        var valueValue = item.BatchSize;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"batch size {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record PrimaryRefreshEstimateCodePrefixRule(string Prefix) : IPrimaryRefreshEstimateRule
{
    public List<string> Check(PrimaryRefreshEstimate item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record PrimaryRefreshEstimateRequiredLabelRule(string Message) : IPrimaryRefreshEstimateRule
{
    public List<string> Check(PrimaryRefreshEstimate item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PrimaryRefreshEstimateBadgeLengthRule(int MaxLength) : IPrimaryRefreshEstimateRule
{
    public List<string> Check(PrimaryRefreshEstimate item)
    {
        var size = item.Badge.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"badge is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a primary refresh estimate.</summary>
public sealed record PrimaryRefreshEstimateValidator(List<IPrimaryRefreshEstimateRule> Rules)
{
    public List<string> Validate(PrimaryRefreshEstimate item)
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

    public void RequireValid(PrimaryRefreshEstimate item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PrimaryRefreshEstimate> items)
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

/// <summary>Functions of primary refresh estimate rules.</summary>
public static class PrimaryRefreshEstimateRules
{
    public static PrimaryRefreshEstimateValidator DefaultPrimaryRefreshEstimateValidator()
    {
        return new PrimaryRefreshEstimateValidator(
            new List<IPrimaryRefreshEstimateRule>
            {
                new PrimaryRefreshEstimateBatchSizeRangeRule(1000, 3600),
                new PrimaryRefreshEstimateCodePrefixRule("ACM"),
                new PrimaryRefreshEstimateRequiredLabelRule("primary refresh estimate needs this field"),
                new PrimaryRefreshEstimateBadgeLengthRule(120),
            });
    }
}
