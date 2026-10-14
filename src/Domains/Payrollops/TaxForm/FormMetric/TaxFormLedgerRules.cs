namespace Acme.Workforce.Payrollops.TaxForm.FormMetric;

/// <summary>A rule a tax form ledger must pass. Check returns its problems, or none.</summary>
public interface ITaxFormLedgerRule
{
    List<string> Check(TaxFormLedger item);
}

public sealed record TaxFormLedgerRequiredCodeRule(string Message) : ITaxFormLedgerRule
{
    public List<string> Check(TaxFormLedger item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record TaxFormLedgerStartsAtRangeRule(int Min, int Max) : ITaxFormLedgerRule
{
    public List<string> Check(TaxFormLedger item)
    {
        var valueValue = item.StartsAt;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"starts at {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"starts at {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record TaxFormLedgerRequiredLocationRule(string Message) : ITaxFormLedgerRule
{
    public List<string> Check(TaxFormLedger item)
    {
        if (item.Location.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record TaxFormLedgerUpdatedAtRangeRule(int Min, int Max) : ITaxFormLedgerRule
{
    public List<string> Check(TaxFormLedger item)
    {
        var valueValue = item.UpdatedAt;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"updated at {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"updated at {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record TaxFormLedgerLocationLengthRule(int MaxLength) : ITaxFormLedgerRule
{
    public List<string> Check(TaxFormLedger item)
    {
        var size = item.Location.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"location is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a tax form ledger.</summary>
public sealed record TaxFormLedgerValidator(List<ITaxFormLedgerRule> Rules)
{
    public List<string> Validate(TaxFormLedger item)
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

    public void RequireValid(TaxFormLedger item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<TaxFormLedger> items)
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

/// <summary>Functions of tax form ledger rules.</summary>
public static class TaxFormLedgerRules
{
    public static TaxFormLedgerValidator DefaultTaxFormLedgerValidator()
    {
        return new TaxFormLedgerValidator(
            new List<ITaxFormLedgerRule>
            {
                new TaxFormLedgerRequiredCodeRule("tax form ledger needs a value"),
                new TaxFormLedgerStartsAtRangeRule(80, 86400),
                new TaxFormLedgerRequiredLocationRule("tax form ledger needs an entry"),
                new TaxFormLedgerUpdatedAtRangeRule(64, 86400),
                new TaxFormLedgerLocationLengthRule(80),
            });
    }
}
