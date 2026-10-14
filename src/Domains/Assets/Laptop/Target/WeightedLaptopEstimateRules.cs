namespace Acme.Workforce.Assets.Laptop.Target;

/// <summary>A rule a weighted laptop estimate must pass. Check returns its problems, or none.</summary>
public interface IWeightedLaptopEstimateRule
{
    List<string> Check(WeightedLaptopEstimate item);
}

public sealed record WeightedLaptopEstimateWeightRangeRule(double Min, double Max) : IWeightedLaptopEstimateRule
{
    public List<string> Check(WeightedLaptopEstimate item)
    {
        var valueValue = item.Weight;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"weight {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record WeightedLaptopEstimateCodePrefixRule(string Prefix) : IWeightedLaptopEstimateRule
{
    public List<string> Check(WeightedLaptopEstimate item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record WeightedLaptopEstimateTitleLengthRule(int MaxLength) : IWeightedLaptopEstimateRule
{
    public List<string> Check(WeightedLaptopEstimate item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record WeightedLaptopEstimateRequiredChecksumRule(string Message) : IWeightedLaptopEstimateRule
{
    public List<string> Check(WeightedLaptopEstimate item)
    {
        if (item.Checksum.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record WeightedLaptopEstimateRequiredCodeRule(string Message) : IWeightedLaptopEstimateRule
{
    public List<string> Check(WeightedLaptopEstimate item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record WeightedLaptopEstimateRequiredTitleRule(string Message) : IWeightedLaptopEstimateRule
{
    public List<string> Check(WeightedLaptopEstimate item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a weighted laptop estimate.</summary>
public sealed record WeightedLaptopEstimateValidator(List<IWeightedLaptopEstimateRule> Rules)
{
    public List<string> Validate(WeightedLaptopEstimate item)
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

    public void RequireValid(WeightedLaptopEstimate item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<WeightedLaptopEstimate> items)
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

/// <summary>Functions of weighted laptop estimate rules.</summary>
public static class WeightedLaptopEstimateRules
{
    public static WeightedLaptopEstimateValidator DefaultWeightedLaptopEstimateValidator()
    {
        return new WeightedLaptopEstimateValidator(
            new List<IWeightedLaptopEstimateRule>
            {
                new WeightedLaptopEstimateWeightRangeRule(1.9, 1409.3),
                new WeightedLaptopEstimateCodePrefixRule("Q"),
                new WeightedLaptopEstimateTitleLengthRule(255),
                new WeightedLaptopEstimateRequiredChecksumRule("weighted laptop estimate needs a value"),
                new WeightedLaptopEstimateRequiredCodeRule("weighted laptop estimate needs a value"),
                new WeightedLaptopEstimateRequiredTitleRule("weighted laptop estimate needs this field"),
            });
    }
}
