namespace Acme.Workforce.Payrollops.PayRun.Bundle;

/// <summary>A rule a pay run summary must pass. Check returns its problems, or none.</summary>
public interface IPayRunSummaryRule
{
    List<string> Check(PayRunSummary item);
}

public sealed record PayRunSummaryCategoryLengthRule(int MaxLength) : IPayRunSummaryRule
{
    public List<string> Check(PayRunSummary item)
    {
        var size = item.Category.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"category is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PayRunSummaryRequiredCodeRule(string Message) : IPayRunSummaryRule
{
    public List<string> Check(PayRunSummary item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PayRunSummaryRequiredTitleRule(string Message) : IPayRunSummaryRule
{
    public List<string> Check(PayRunSummary item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PayRunSummaryCodePrefixRule(string Prefix) : IPayRunSummaryRule
{
    public List<string> Check(PayRunSummary item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record PayRunSummaryUpdatedAtRangeRule(int Min, int Max) : IPayRunSummaryRule
{
    public List<string> Check(PayRunSummary item)
    {
        var valueValue = item.UpdatedAt;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"updated at {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record PayRunSummaryRequiredSegmentRule(string Message) : IPayRunSummaryRule
{
    public List<string> Check(PayRunSummary item)
    {
        if (item.Segment.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a pay run summary.</summary>
public sealed record PayRunSummaryValidator(List<IPayRunSummaryRule> Rules)
{
    public List<string> Validate(PayRunSummary item)
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

    public void RequireValid(PayRunSummary item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PayRunSummary> items)
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

/// <summary>Functions of pay run summary rules.</summary>
public static class PayRunSummaryRules
{
    public static PayRunSummaryValidator DefaultPayRunSummaryValidator()
    {
        return new PayRunSummaryValidator(
            new List<IPayRunSummaryRule>
            {
                new PayRunSummaryCategoryLengthRule(16),
                new PayRunSummaryRequiredCodeRule("pay run summary needs an entry"),
                new PayRunSummaryRequiredTitleRule("pay run summary needs this field"),
                new PayRunSummaryCodePrefixRule("Q"),
                new PayRunSummaryUpdatedAtRangeRule(1000, 86400),
                new PayRunSummaryRequiredSegmentRule("pay run summary needs an entry"),
            });
    }
}
