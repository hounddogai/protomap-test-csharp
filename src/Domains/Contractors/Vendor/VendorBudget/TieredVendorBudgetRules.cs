namespace Acme.Workforce.Contractors.Vendor.VendorBudget;

/// <summary>A rule a tiered vendor budget must pass. Check returns its problems, or none.</summary>
public interface ITieredVendorBudgetRule
{
    List<string> Check(TieredVendorBudget item);
}

public sealed record TieredVendorBudgetCodePrefixRule(string Prefix) : ITieredVendorBudgetRule
{
    public List<string> Check(TieredVendorBudget item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record TieredVendorBudgetLabelLengthRule(int MaxLength) : ITieredVendorBudgetRule
{
    public List<string> Check(TieredVendorBudget item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record TieredVendorBudgetRequiredCodeRule(string Message) : ITieredVendorBudgetRule
{
    public List<string> Check(TieredVendorBudget item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record TieredVendorBudgetCodeLengthRule(int MaxLength) : ITieredVendorBudgetRule
{
    public List<string> Check(TieredVendorBudget item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a tiered vendor budget.</summary>
public sealed record TieredVendorBudgetValidator(List<ITieredVendorBudgetRule> Rules)
{
    public List<string> Validate(TieredVendorBudget item)
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

    public void RequireValid(TieredVendorBudget item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<TieredVendorBudget> items)
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

/// <summary>Functions of tiered vendor budget rules.</summary>
public static class TieredVendorBudgetRules
{
    public static TieredVendorBudgetValidator DefaultTieredVendorBudgetValidator()
    {
        return new TieredVendorBudgetValidator(
            new List<ITieredVendorBudgetRule>
            {
                new TieredVendorBudgetCodePrefixRule("ACM"),
                new TieredVendorBudgetLabelLengthRule(32),
                new TieredVendorBudgetRequiredCodeRule("tiered vendor budget needs a value"),
                new TieredVendorBudgetCodeLengthRule(64),
            });
    }
}
