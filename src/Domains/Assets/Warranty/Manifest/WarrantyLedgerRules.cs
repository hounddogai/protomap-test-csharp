namespace Acme.Workforce.Assets.Warranty.Manifest;

/// <summary>A rule a warranty ledger must pass. Check returns its problems, or none.</summary>
public interface IWarrantyLedgerRule
{
    List<string> Check(WarrantyLedger item);
}

public sealed record WarrantyLedgerTitleLengthRule(int MaxLength) : IWarrantyLedgerRule
{
    public List<string> Check(WarrantyLedger item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record WarrantyLedgerSalaryBandLengthRule(int MaxLength) : IWarrantyLedgerRule
{
    public List<string> Check(WarrantyLedger item)
    {
        var size = item.SalaryBand.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"salary band is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record WarrantyLedgerDescriptionLengthRule(int MaxLength) : IWarrantyLedgerRule
{
    public List<string> Check(WarrantyLedger item)
    {
        var size = item.Description.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"description is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record WarrantyLedgerCodePrefixRule(string Prefix) : IWarrantyLedgerRule
{
    public List<string> Check(WarrantyLedger item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a warranty ledger.</summary>
public sealed record WarrantyLedgerValidator(List<IWarrantyLedgerRule> Rules)
{
    public List<string> Validate(WarrantyLedger item)
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

    public void RequireValid(WarrantyLedger item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<WarrantyLedger> items)
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

/// <summary>Functions of warranty ledger rules.</summary>
public static class WarrantyLedgerRules
{
    public static WarrantyLedgerValidator DefaultWarrantyLedgerValidator()
    {
        return new WarrantyLedgerValidator(
            new List<IWarrantyLedgerRule>
            {
                new WarrantyLedgerTitleLengthRule(255),
                new WarrantyLedgerSalaryBandLengthRule(86400),
                new WarrantyLedgerDescriptionLengthRule(1000),
                new WarrantyLedgerCodePrefixRule("Q"),
            });
    }
}
