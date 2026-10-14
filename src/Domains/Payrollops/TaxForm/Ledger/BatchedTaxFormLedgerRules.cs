namespace Acme.Workforce.Payrollops.TaxForm.Ledger;

/// <summary>A rule a batched tax form ledger must pass. Check returns its problems, or none.</summary>
public interface IBatchedTaxFormLedgerRule
{
    List<string> Check(BatchedTaxFormLedger item);
}

public sealed record BatchedTaxFormLedgerCostCenterLengthRule(int MaxLength) : IBatchedTaxFormLedgerRule
{
    public List<string> Check(BatchedTaxFormLedger item)
    {
        var size = item.CostCenter.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"cost center is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record BatchedTaxFormLedgerMarginRangeRule(double Min, double Max) : IBatchedTaxFormLedgerRule
{
    public List<string> Check(BatchedTaxFormLedger item)
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

public sealed record BatchedTaxFormLedgerCodePrefixRule(string Prefix) : IBatchedTaxFormLedgerRule
{
    public List<string> Check(BatchedTaxFormLedger item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a batched tax form ledger.</summary>
public sealed record BatchedTaxFormLedgerValidator(List<IBatchedTaxFormLedgerRule> Rules)
{
    public List<string> Validate(BatchedTaxFormLedger item)
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

    public void RequireValid(BatchedTaxFormLedger item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<BatchedTaxFormLedger> items)
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

/// <summary>Functions of batched tax form ledger rules.</summary>
public static class BatchedTaxFormLedgerRules
{
    public static BatchedTaxFormLedgerValidator DefaultBatchedTaxFormLedgerValidator()
    {
        return new BatchedTaxFormLedgerValidator(
            new List<IBatchedTaxFormLedgerRule>
            {
                new BatchedTaxFormLedgerCostCenterLengthRule(80),
                new BatchedTaxFormLedgerMarginRangeRule(0.9, 3633.7),
                new BatchedTaxFormLedgerCodePrefixRule("OPS"),
            });
    }
}
