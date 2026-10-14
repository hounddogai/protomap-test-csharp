namespace Acme.Workforce.Contractors.Offboarding.OffboardingThreshold;

/// <summary>A rule a offboarding ledger must pass. Check returns its problems, or none.</summary>
public interface IOffboardingLedgerRule
{
    List<string> Check(OffboardingLedger item);
}

public sealed record OffboardingLedgerCategoryLengthRule(int MaxLength) : IOffboardingLedgerRule
{
    public List<string> Check(OffboardingLedger item)
    {
        var size = item.Category.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"category is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record OffboardingLedgerRevisionRangeRule(int Min, int Max) : IOffboardingLedgerRule
{
    public List<string> Check(OffboardingLedger item)
    {
        var valueValue = item.Revision;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"revision {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"revision {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record OffboardingLedgerTitleLengthRule(int MaxLength) : IOffboardingLedgerRule
{
    public List<string> Check(OffboardingLedger item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record OffboardingLedgerMarginRangeRule(double Min, double Max) : IOffboardingLedgerRule
{
    public List<string> Check(OffboardingLedger item)
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

public sealed record OffboardingLedgerRequiredCodeRule(string Message) : IOffboardingLedgerRule
{
    public List<string> Check(OffboardingLedger item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record OffboardingLedgerCodePrefixRule(string Prefix) : IOffboardingLedgerRule
{
    public List<string> Check(OffboardingLedger item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a offboarding ledger.</summary>
public sealed record OffboardingLedgerValidator(List<IOffboardingLedgerRule> Rules)
{
    public List<string> Validate(OffboardingLedger item)
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

    public void RequireValid(OffboardingLedger item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<OffboardingLedger> items)
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

/// <summary>Functions of offboarding ledger rules.</summary>
public static class OffboardingLedgerRules
{
    public static OffboardingLedgerValidator DefaultOffboardingLedgerValidator()
    {
        return new OffboardingLedgerValidator(
            new List<IOffboardingLedgerRule>
            {
                new OffboardingLedgerCategoryLengthRule(120),
                new OffboardingLedgerRevisionRangeRule(16, 3600),
                new OffboardingLedgerTitleLengthRule(16),
                new OffboardingLedgerMarginRangeRule(6.8, 772.6),
                new OffboardingLedgerRequiredCodeRule("offboarding ledger needs an entry"),
                new OffboardingLedgerCodePrefixRule("OPS"),
            });
    }
}
