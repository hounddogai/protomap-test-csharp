namespace Acme.Workforce.Absence.Accrual.Digest;

/// <summary>A rule a regional accrual summary must pass. Check returns its problems, or none.</summary>
public interface IRegionalAccrualSummaryRule
{
    List<string> Check(RegionalAccrualSummary item);
}

public sealed record RegionalAccrualSummaryTitleLengthRule(int MaxLength) : IRegionalAccrualSummaryRule
{
    public List<string> Check(RegionalAccrualSummary item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record RegionalAccrualSummaryCodePrefixRule(string Prefix) : IRegionalAccrualSummaryRule
{
    public List<string> Check(RegionalAccrualSummary item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record RegionalAccrualSummaryCategoryLengthRule(int MaxLength) : IRegionalAccrualSummaryRule
{
    public List<string> Check(RegionalAccrualSummary item)
    {
        var size = item.Category.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"category is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record RegionalAccrualSummaryRequiredCategoryRule(string Message) : IRegionalAccrualSummaryRule
{
    public List<string> Check(RegionalAccrualSummary item)
    {
        if (item.Category.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record RegionalAccrualSummaryCodeLengthRule(int MaxLength) : IRegionalAccrualSummaryRule
{
    public List<string> Check(RegionalAccrualSummary item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a regional accrual summary.</summary>
public sealed record RegionalAccrualSummaryValidator(List<IRegionalAccrualSummaryRule> Rules)
{
    public List<string> Validate(RegionalAccrualSummary item)
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

    public void RequireValid(RegionalAccrualSummary item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<RegionalAccrualSummary> items)
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

/// <summary>Functions of regional accrual summary rules.</summary>
public static class RegionalAccrualSummaryRules
{
    public static RegionalAccrualSummaryValidator DefaultRegionalAccrualSummaryValidator()
    {
        return new RegionalAccrualSummaryValidator(
            new List<IRegionalAccrualSummaryRule>
            {
                new RegionalAccrualSummaryTitleLengthRule(64),
                new RegionalAccrualSummaryCodePrefixRule("X"),
                new RegionalAccrualSummaryCategoryLengthRule(80),
                new RegionalAccrualSummaryRequiredCategoryRule("regional accrual summary needs an entry"),
                new RegionalAccrualSummaryCodeLengthRule(32),
            });
    }
}
