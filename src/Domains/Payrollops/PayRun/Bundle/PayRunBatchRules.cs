namespace Acme.Workforce.Payrollops.PayRun.Bundle;

/// <summary>A rule a pay run batch must pass. Check returns its problems, or none.</summary>
public interface IPayRunBatchRule
{
    List<string> Check(PayRunBatch item);
}

public sealed record PayRunBatchLabelLengthRule(int MaxLength) : IPayRunBatchRule
{
    public List<string> Check(PayRunBatch item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PayRunBatchRequiredBadgeRule(string Message) : IPayRunBatchRule
{
    public List<string> Check(PayRunBatch item)
    {
        if (item.Badge.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PayRunBatchRequiredLabelRule(string Message) : IPayRunBatchRule
{
    public List<string> Check(PayRunBatch item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PayRunBatchRequiredCodeRule(string Message) : IPayRunBatchRule
{
    public List<string> Check(PayRunBatch item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PayRunBatchBadgeLengthRule(int MaxLength) : IPayRunBatchRule
{
    public List<string> Check(PayRunBatch item)
    {
        var size = item.Badge.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"badge is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PayRunBatchCodePrefixRule(string Prefix) : IPayRunBatchRule
{
    public List<string> Check(PayRunBatch item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a pay run batch.</summary>
public sealed record PayRunBatchValidator(List<IPayRunBatchRule> Rules)
{
    public List<string> Validate(PayRunBatch item)
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

    public void RequireValid(PayRunBatch item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PayRunBatch> items)
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

/// <summary>Functions of pay run batch rules.</summary>
public static class PayRunBatchRules
{
    public static PayRunBatchValidator DefaultPayRunBatchValidator()
    {
        return new PayRunBatchValidator(
            new List<IPayRunBatchRule>
            {
                new PayRunBatchLabelLengthRule(16),
                new PayRunBatchRequiredBadgeRule("pay run batch needs an entry"),
                new PayRunBatchRequiredLabelRule("pay run batch needs a value"),
                new PayRunBatchRequiredCodeRule("pay run batch needs a value"),
                new PayRunBatchBadgeLengthRule(1000),
                new PayRunBatchCodePrefixRule("ACM"),
            });
    }
}
