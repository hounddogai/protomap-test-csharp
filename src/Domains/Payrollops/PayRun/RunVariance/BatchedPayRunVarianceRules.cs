namespace Acme.Workforce.Payrollops.PayRun.RunVariance;

/// <summary>A rule a batched pay run variance must pass. Check returns its problems, or none.</summary>
public interface IBatchedPayRunVarianceRule
{
    List<string> Check(BatchedPayRunVariance item);
}

public sealed record BatchedPayRunVarianceRequiredCodeRule(string Message) : IBatchedPayRunVarianceRule
{
    public List<string> Check(BatchedPayRunVariance item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record BatchedPayRunVarianceCodeLengthRule(int MaxLength) : IBatchedPayRunVarianceRule
{
    public List<string> Check(BatchedPayRunVariance item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record BatchedPayRunVarianceDiscountRangeRule(double Min, double Max) : IBatchedPayRunVarianceRule
{
    public List<string> Check(BatchedPayRunVariance item)
    {
        var valueValue = item.Discount;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"discount {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record BatchedPayRunVarianceCodePrefixRule(string Prefix) : IBatchedPayRunVarianceRule
{
    public List<string> Check(BatchedPayRunVariance item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record BatchedPayRunVarianceRequiredTeamRule(string Message) : IBatchedPayRunVarianceRule
{
    public List<string> Check(BatchedPayRunVariance item)
    {
        if (item.Team.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a batched pay run variance.</summary>
public sealed record BatchedPayRunVarianceValidator(List<IBatchedPayRunVarianceRule> Rules)
{
    public List<string> Validate(BatchedPayRunVariance item)
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

    public void RequireValid(BatchedPayRunVariance item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<BatchedPayRunVariance> items)
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

/// <summary>Functions of batched pay run variance rules.</summary>
public static class BatchedPayRunVarianceRules
{
    public static BatchedPayRunVarianceValidator DefaultBatchedPayRunVarianceValidator()
    {
        return new BatchedPayRunVarianceValidator(
            new List<IBatchedPayRunVarianceRule>
            {
                new BatchedPayRunVarianceRequiredCodeRule("batched pay run variance needs an entry"),
                new BatchedPayRunVarianceCodeLengthRule(3600),
                new BatchedPayRunVarianceDiscountRangeRule(1.9, 302.0),
                new BatchedPayRunVarianceCodePrefixRule("X"),
                new BatchedPayRunVarianceRequiredTeamRule("batched pay run variance needs this field"),
            });
    }
}
