namespace Acme.Workforce.Compliance.Exception.Cycle;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a exception threshold must pass. Check returns its problems, or none.</summary>
public interface IExceptionThresholdRule
{
    List<string> Check(ExceptionThreshold item);
}

public sealed record ExceptionThresholdApprovedGuardRule(int Limit) : IExceptionThresholdRule
{
    public List<string> Check(ExceptionThreshold item)
    {
        if (item.Approved && item.Quantity > Limit)
        {
            return new List<string> { $"quantity {item.Quantity} exceeds {Limit} while approved" };
        }
        return new List<string>();
    }
}

public sealed record ExceptionThresholdQuantityRangeRule(int Min, int Max) : IExceptionThresholdRule
{
    public List<string> Check(ExceptionThreshold item)
    {
        var valueValue = item.Quantity;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"quantity {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record ExceptionThresholdCodePrefixRule(string Prefix) : IExceptionThresholdRule
{
    public List<string> Check(ExceptionThreshold item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record ExceptionThresholdAllowedReviewerRule(List<string> Allowed) : IExceptionThresholdRule
{
    public List<string> Check(ExceptionThreshold item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Reviewer)))
        {
            return new List<string> { $"reviewer {item.Reviewer} is out of range" };
        }
        return new List<string>();
    }
}

public sealed record ExceptionThresholdRequiredCurrencyRule(string Message) : IExceptionThresholdRule
{
    public List<string> Check(ExceptionThreshold item)
    {
        if (item.Currency.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ExceptionThresholdCurrencyLengthRule(int MaxLength) : IExceptionThresholdRule
{
    public List<string> Check(ExceptionThreshold item)
    {
        var size = item.Currency.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"currency is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a exception threshold.</summary>
public sealed record ExceptionThresholdValidator(List<IExceptionThresholdRule> Rules)
{
    public List<string> Validate(ExceptionThreshold item)
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

    public void RequireValid(ExceptionThreshold item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ExceptionThreshold> items)
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

/// <summary>Functions of exception threshold rules.</summary>
public static class ExceptionThresholdRules
{
    public static ExceptionThresholdValidator DefaultExceptionThresholdValidator()
    {
        return new ExceptionThresholdValidator(
            new List<IExceptionThresholdRule>
            {
                new ExceptionThresholdApprovedGuardRule(255),
                new ExceptionThresholdQuantityRangeRule(32, 1000),
                new ExceptionThresholdCodePrefixRule("REF"),
                new ExceptionThresholdAllowedReviewerRule(new List<string> { "regional" }),
                new ExceptionThresholdRequiredCurrencyRule("exception threshold needs an entry"),
                new ExceptionThresholdCurrencyLengthRule(16),
            });
    }
}
