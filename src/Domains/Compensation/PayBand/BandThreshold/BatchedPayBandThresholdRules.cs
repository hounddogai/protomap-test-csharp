namespace Acme.Workforce.Compensation.PayBand.BandThreshold;

/// <summary>A rule a batched pay band threshold must pass. Check returns its problems, or none.</summary>
public interface IBatchedPayBandThresholdRule
{
    List<string> Check(BatchedPayBandThreshold item);
}

public sealed record BatchedPayBandThresholdCodeLengthRule(int MaxLength) : IBatchedPayBandThresholdRule
{
    public List<string> Check(BatchedPayBandThreshold item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record BatchedPayBandThresholdRequiredCountryRule(string Message) : IBatchedPayBandThresholdRule
{
    public List<string> Check(BatchedPayBandThreshold item)
    {
        if (item.Country.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record BatchedPayBandThresholdMarginRangeRule(double Min, double Max) : IBatchedPayBandThresholdRule
{
    public List<string> Check(BatchedPayBandThreshold item)
    {
        var valueValue = item.Margin;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"margin {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"margin {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record BatchedPayBandThresholdUnitLengthRule(int MaxLength) : IBatchedPayBandThresholdRule
{
    public List<string> Check(BatchedPayBandThreshold item)
    {
        var size = item.Unit.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"unit is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record BatchedPayBandThresholdCodePrefixRule(string Prefix) : IBatchedPayBandThresholdRule
{
    public List<string> Check(BatchedPayBandThreshold item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a batched pay band threshold.</summary>
public sealed record BatchedPayBandThresholdValidator(List<IBatchedPayBandThresholdRule> Rules)
{
    public List<string> Validate(BatchedPayBandThreshold item)
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

    public void RequireValid(BatchedPayBandThreshold item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<BatchedPayBandThreshold> items)
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

/// <summary>Functions of batched pay band threshold rules.</summary>
public static class BatchedPayBandThresholdRules
{
    public static BatchedPayBandThresholdValidator DefaultBatchedPayBandThresholdValidator()
    {
        return new BatchedPayBandThresholdValidator(
            new List<IBatchedPayBandThresholdRule>
            {
                new BatchedPayBandThresholdCodeLengthRule(64),
                new BatchedPayBandThresholdRequiredCountryRule("batched pay band threshold needs a value"),
                new BatchedPayBandThresholdMarginRangeRule(0.1, 2017.4),
                new BatchedPayBandThresholdUnitLengthRule(120),
                new BatchedPayBandThresholdCodePrefixRule("Q"),
            });
    }
}
