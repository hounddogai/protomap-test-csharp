namespace Acme.Workforce.Workplace.AccessBadge.Notice;

/// <summary>A rule a primary access badge summary must pass. Check returns its problems, or none.</summary>
public interface IPrimaryAccessBadgeSummaryRule
{
    List<string> Check(PrimaryAccessBadgeSummary item);
}

public sealed record PrimaryAccessBadgeSummaryRequiredLocaleRule(string Message) : IPrimaryAccessBadgeSummaryRule
{
    public List<string> Check(PrimaryAccessBadgeSummary item)
    {
        if (item.Locale.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PrimaryAccessBadgeSummaryCurrencyLengthRule(int MaxLength) : IPrimaryAccessBadgeSummaryRule
{
    public List<string> Check(PrimaryAccessBadgeSummary item)
    {
        var size = item.Currency.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"currency is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PrimaryAccessBadgeSummaryWeightRangeRule(double Min, double Max) : IPrimaryAccessBadgeSummaryRule
{
    public List<string> Check(PrimaryAccessBadgeSummary item)
    {
        var valueValue = item.Weight;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"weight {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"weight {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record PrimaryAccessBadgeSummaryCodePrefixRule(string Prefix) : IPrimaryAccessBadgeSummaryRule
{
    public List<string> Check(PrimaryAccessBadgeSummary item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record PrimaryAccessBadgeSummaryPinnedGuardRule(double Limit) : IPrimaryAccessBadgeSummaryRule
{
    public List<string> Check(PrimaryAccessBadgeSummary item)
    {
        if (item.Pinned && item.Weight > Limit)
        {
            return new List<string> { $"weight {item.Weight} exceeds {Limit} while pinned" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a primary access badge summary.</summary>
public sealed record PrimaryAccessBadgeSummaryValidator(List<IPrimaryAccessBadgeSummaryRule> Rules)
{
    public List<string> Validate(PrimaryAccessBadgeSummary item)
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

    public void RequireValid(PrimaryAccessBadgeSummary item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PrimaryAccessBadgeSummary> items)
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

/// <summary>Functions of primary access badge summary rules.</summary>
public static class PrimaryAccessBadgeSummaryRules
{
    public static PrimaryAccessBadgeSummaryValidator DefaultPrimaryAccessBadgeSummaryValidator()
    {
        return new PrimaryAccessBadgeSummaryValidator(
            new List<IPrimaryAccessBadgeSummaryRule>
            {
                new PrimaryAccessBadgeSummaryRequiredLocaleRule("primary access badge summary needs an entry"),
                new PrimaryAccessBadgeSummaryCurrencyLengthRule(3600),
                new PrimaryAccessBadgeSummaryWeightRangeRule(9.9, 667.7),
                new PrimaryAccessBadgeSummaryCodePrefixRule("ACM"),
                new PrimaryAccessBadgeSummaryPinnedGuardRule(6.1),
            });
    }
}
