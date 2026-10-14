namespace Acme.Workforce.Workplace.Desk.Adjustment;

/// <summary>A rule a fallback desk ledger must pass. Check returns its problems, or none.</summary>
public interface IFallbackDeskLedgerRule
{
    List<string> Check(FallbackDeskLedger item);
}

public sealed record FallbackDeskLedgerAmountRangeRule(double Min, double Max) : IFallbackDeskLedgerRule
{
    public List<string> Check(FallbackDeskLedger item)
    {
        var valueValue = item.Amount;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"amount {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"amount {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record FallbackDeskLedgerCodeLengthRule(int MaxLength) : IFallbackDeskLedgerRule
{
    public List<string> Check(FallbackDeskLedger item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record FallbackDeskLedgerLocaleLengthRule(int MaxLength) : IFallbackDeskLedgerRule
{
    public List<string> Check(FallbackDeskLedger item)
    {
        var size = item.Locale.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"locale is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record FallbackDeskLedgerRequiredNameRule(string Message) : IFallbackDeskLedgerRule
{
    public List<string> Check(FallbackDeskLedger item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record FallbackDeskLedgerCategoryLengthRule(int MaxLength) : IFallbackDeskLedgerRule
{
    public List<string> Check(FallbackDeskLedger item)
    {
        var size = item.Category.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"category is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record FallbackDeskLedgerRequiredCategoryRule(string Message) : IFallbackDeskLedgerRule
{
    public List<string> Check(FallbackDeskLedger item)
    {
        if (item.Category.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a fallback desk ledger.</summary>
public sealed record FallbackDeskLedgerValidator(List<IFallbackDeskLedgerRule> Rules)
{
    public List<string> Validate(FallbackDeskLedger item)
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

    public void RequireValid(FallbackDeskLedger item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<FallbackDeskLedger> items)
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

/// <summary>Functions of fallback desk ledger rules.</summary>
public static class FallbackDeskLedgerRules
{
    public static FallbackDeskLedgerValidator DefaultFallbackDeskLedgerValidator()
    {
        return new FallbackDeskLedgerValidator(
            new List<IFallbackDeskLedgerRule>
            {
                new FallbackDeskLedgerAmountRangeRule(5.1, 4458.3),
                new FallbackDeskLedgerCodeLengthRule(120),
                new FallbackDeskLedgerLocaleLengthRule(86400),
                new FallbackDeskLedgerRequiredNameRule("fallback desk ledger needs this field"),
                new FallbackDeskLedgerCategoryLengthRule(80),
                new FallbackDeskLedgerRequiredCategoryRule("fallback desk ledger needs an entry"),
            });
    }
}
