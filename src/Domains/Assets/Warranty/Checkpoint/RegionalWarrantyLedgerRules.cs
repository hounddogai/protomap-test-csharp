namespace Acme.Workforce.Assets.Warranty.Checkpoint;

/// <summary>A rule a regional warranty ledger must pass. Check returns its problems, or none.</summary>
public interface IRegionalWarrantyLedgerRule
{
    List<string> Check(RegionalWarrantyLedger item);
}

public sealed record RegionalWarrantyLedgerCodePrefixRule(string Prefix) : IRegionalWarrantyLedgerRule
{
    public List<string> Check(RegionalWarrantyLedger item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record RegionalWarrantyLedgerCostCenterLengthRule(int MaxLength) : IRegionalWarrantyLedgerRule
{
    public List<string> Check(RegionalWarrantyLedger item)
    {
        var size = item.CostCenter.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"cost center is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record RegionalWarrantyLedgerRateRangeRule(double Min, double Max) : IRegionalWarrantyLedgerRule
{
    public List<string> Check(RegionalWarrantyLedger item)
    {
        var valueValue = item.Rate;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"rate {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record RegionalWarrantyLedgerRequiredNameRule(string Message) : IRegionalWarrantyLedgerRule
{
    public List<string> Check(RegionalWarrantyLedger item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a regional warranty ledger.</summary>
public sealed record RegionalWarrantyLedgerValidator(List<IRegionalWarrantyLedgerRule> Rules)
{
    public List<string> Validate(RegionalWarrantyLedger item)
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

    public void RequireValid(RegionalWarrantyLedger item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<RegionalWarrantyLedger> items)
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

/// <summary>Functions of regional warranty ledger rules.</summary>
public static class RegionalWarrantyLedgerRules
{
    public static RegionalWarrantyLedgerValidator DefaultRegionalWarrantyLedgerValidator()
    {
        return new RegionalWarrantyLedgerValidator(
            new List<IRegionalWarrantyLedgerRule>
            {
                new RegionalWarrantyLedgerCodePrefixRule("OPS"),
                new RegionalWarrantyLedgerCostCenterLengthRule(64),
                new RegionalWarrantyLedgerRateRangeRule(0.4, 619.0),
                new RegionalWarrantyLedgerRequiredNameRule("regional warranty ledger needs an entry"),
            });
    }
}
