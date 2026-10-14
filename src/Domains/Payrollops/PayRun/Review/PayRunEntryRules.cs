namespace Acme.Workforce.Payrollops.PayRun.Review;

/// <summary>A rule a pay run entry must pass. Check returns its problems, or none.</summary>
public interface IPayRunEntryRule
{
    List<string> Check(PayRunEntry item);
}

public sealed record PayRunEntryGraceDaysRangeRule(int Min, int Max) : IPayRunEntryRule
{
    public List<string> Check(PayRunEntry item)
    {
        var valueValue = item.GraceDays;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"grace days {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"grace days {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record PayRunEntryCodePrefixRule(string Prefix) : IPayRunEntryRule
{
    public List<string> Check(PayRunEntry item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record PayRunEntryRequiredTitleRule(string Message) : IPayRunEntryRule
{
    public List<string> Check(PayRunEntry item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a pay run entry.</summary>
public sealed record PayRunEntryValidator(List<IPayRunEntryRule> Rules)
{
    public List<string> Validate(PayRunEntry item)
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

    public void RequireValid(PayRunEntry item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PayRunEntry> items)
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

/// <summary>Functions of pay run entry rules.</summary>
public static class PayRunEntryRules
{
    public static PayRunEntryValidator DefaultPayRunEntryValidator()
    {
        return new PayRunEntryValidator(
            new List<IPayRunEntryRule>
            {
                new PayRunEntryGraceDaysRangeRule(32, 3600),
                new PayRunEntryCodePrefixRule("ACM"),
                new PayRunEntryRequiredTitleRule("pay run entry needs this field"),
            });
    }
}
