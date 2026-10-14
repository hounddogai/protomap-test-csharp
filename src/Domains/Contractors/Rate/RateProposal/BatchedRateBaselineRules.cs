namespace Acme.Workforce.Contractors.Rate.RateProposal;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a batched rate baseline must pass. Check returns its problems, or none.</summary>
public interface IBatchedRateBaselineRule
{
    List<string> Check(BatchedRateBaseline item);
}

public sealed record BatchedRateBaselineDiscountRangeRule(double Min, double Max) : IBatchedRateBaselineRule
{
    public List<string> Check(BatchedRateBaseline item)
    {
        var valueValue = item.Discount;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"discount {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record BatchedRateBaselineCodeLengthRule(int MaxLength) : IBatchedRateBaselineRule
{
    public List<string> Check(BatchedRateBaseline item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record BatchedRateBaselineAllowedTimezoneRule(List<string> Allowed) : IBatchedRateBaselineRule
{
    public List<string> Check(BatchedRateBaseline item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Timezone)))
        {
            return new List<string> { $"timezone {item.Timezone} is expired" };
        }
        return new List<string>();
    }
}

public sealed record BatchedRateBaselineCodePrefixRule(string Prefix) : IBatchedRateBaselineRule
{
    public List<string> Check(BatchedRateBaseline item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record BatchedRateBaselineRequiredCodeRule(string Message) : IBatchedRateBaselineRule
{
    public List<string> Check(BatchedRateBaseline item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record BatchedRateBaselineTitleLengthRule(int MaxLength) : IBatchedRateBaselineRule
{
    public List<string> Check(BatchedRateBaseline item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a batched rate baseline.</summary>
public sealed record BatchedRateBaselineValidator(List<IBatchedRateBaselineRule> Rules)
{
    public List<string> Validate(BatchedRateBaseline item)
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

    public void RequireValid(BatchedRateBaseline item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<BatchedRateBaseline> items)
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

/// <summary>Functions of batched rate baseline rules.</summary>
public static class BatchedRateBaselineRules
{
    public static BatchedRateBaselineValidator DefaultBatchedRateBaselineValidator()
    {
        return new BatchedRateBaselineValidator(
            new List<IBatchedRateBaselineRule>
            {
                new BatchedRateBaselineDiscountRangeRule(8.9, 4186.8),
                new BatchedRateBaselineCodeLengthRule(120),
                new BatchedRateBaselineAllowedTimezoneRule(new List<string>()),
                new BatchedRateBaselineCodePrefixRule("REF"),
                new BatchedRateBaselineRequiredCodeRule("batched rate baseline needs an entry"),
                new BatchedRateBaselineTitleLengthRule(80),
            });
    }
}
