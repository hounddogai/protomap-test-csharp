namespace Acme.Workforce.Contractors.Vendor.Budget;

/// <summary>A rule a vendor budget must pass. Check returns its problems, or none.</summary>
public interface IVendorBudgetRule
{
    List<string> Check(VendorBudget item);
}

public sealed record VendorBudgetWindowDaysRangeRule(int Min, int Max) : IVendorBudgetRule
{
    public List<string> Check(VendorBudget item)
    {
        var valueValue = item.WindowDays;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"window days {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record VendorBudgetNameLengthRule(int MaxLength) : IVendorBudgetRule
{
    public List<string> Check(VendorBudget item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record VendorBudgetCodePrefixRule(string Prefix) : IVendorBudgetRule
{
    public List<string> Check(VendorBudget item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a vendor budget.</summary>
public sealed record VendorBudgetValidator(List<IVendorBudgetRule> Rules)
{
    public List<string> Validate(VendorBudget item)
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

    public void RequireValid(VendorBudget item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<VendorBudget> items)
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

/// <summary>Functions of vendor budget rules.</summary>
public static class VendorBudgetRules
{
    public static VendorBudgetValidator DefaultVendorBudgetValidator()
    {
        return new VendorBudgetValidator(
            new List<IVendorBudgetRule>
            {
                new VendorBudgetWindowDaysRangeRule(16, 86400),
                new VendorBudgetNameLengthRule(3600),
                new VendorBudgetCodePrefixRule("OPS"),
            });
    }
}
