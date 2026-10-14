namespace Acme.Workforce.Contractors.Vendor.Rollup;

/// <summary>A rule a vendor ledger must pass. Check returns its problems, or none.</summary>
public interface IVendorLedgerRule
{
    List<string> Check(VendorLedger item);
}

public sealed record VendorLedgerMarginRangeRule(double Min, double Max) : IVendorLedgerRule
{
    public List<string> Check(VendorLedger item)
    {
        var valueValue = item.Margin;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"margin {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record VendorLedgerRequiredTeamRule(string Message) : IVendorLedgerRule
{
    public List<string> Check(VendorLedger item)
    {
        if (item.Team.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record VendorLedgerRequiredSkillRule(string Message) : IVendorLedgerRule
{
    public List<string> Check(VendorLedger item)
    {
        if (item.Skill.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record VendorLedgerTitleLengthRule(int MaxLength) : IVendorLedgerRule
{
    public List<string> Check(VendorLedger item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a vendor ledger.</summary>
public sealed record VendorLedgerValidator(List<IVendorLedgerRule> Rules)
{
    public List<string> Validate(VendorLedger item)
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

    public void RequireValid(VendorLedger item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<VendorLedger> items)
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

/// <summary>Functions of vendor ledger rules.</summary>
public static class VendorLedgerRules
{
    public static VendorLedgerValidator DefaultVendorLedgerValidator()
    {
        return new VendorLedgerValidator(
            new List<IVendorLedgerRule>
            {
                new VendorLedgerMarginRangeRule(2.5, 2597.0),
                new VendorLedgerRequiredTeamRule("vendor ledger needs an entry"),
                new VendorLedgerRequiredSkillRule("vendor ledger needs an entry"),
                new VendorLedgerTitleLengthRule(3600),
            });
    }
}
