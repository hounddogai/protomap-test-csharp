namespace Acme.Workforce.Contractors.Vendor.VendorSegment;

/// <summary>A rule a legacy vendor budget must pass. Check returns its problems, or none.</summary>
public interface ILegacyVendorBudgetRule
{
    List<string> Check(LegacyVendorBudget item);
}

public sealed record LegacyVendorBudgetRequiredNameRule(string Message) : ILegacyVendorBudgetRule
{
    public List<string> Check(LegacyVendorBudget item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record LegacyVendorBudgetWeightRangeRule(double Min, double Max) : ILegacyVendorBudgetRule
{
    public List<string> Check(LegacyVendorBudget item)
    {
        var valueValue = item.Weight;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"weight {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record LegacyVendorBudgetUnitLengthRule(int MaxLength) : ILegacyVendorBudgetRule
{
    public List<string> Check(LegacyVendorBudget item)
    {
        var size = item.Unit.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"unit is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LegacyVendorBudgetRequiredUnitRule(string Message) : ILegacyVendorBudgetRule
{
    public List<string> Check(LegacyVendorBudget item)
    {
        if (item.Unit.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record LegacyVendorBudgetRequiredRegionRule(string Message) : ILegacyVendorBudgetRule
{
    public List<string> Check(LegacyVendorBudget item)
    {
        if (item.Region.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a legacy vendor budget.</summary>
public sealed record LegacyVendorBudgetValidator(List<ILegacyVendorBudgetRule> Rules)
{
    public List<string> Validate(LegacyVendorBudget item)
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

    public void RequireValid(LegacyVendorBudget item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<LegacyVendorBudget> items)
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

/// <summary>Functions of legacy vendor budget rules.</summary>
public static class LegacyVendorBudgetRules
{
    public static LegacyVendorBudgetValidator DefaultLegacyVendorBudgetValidator()
    {
        return new LegacyVendorBudgetValidator(
            new List<ILegacyVendorBudgetRule>
            {
                new LegacyVendorBudgetRequiredNameRule("legacy vendor budget needs a value"),
                new LegacyVendorBudgetWeightRangeRule(5.1, 1829.2),
                new LegacyVendorBudgetUnitLengthRule(80),
                new LegacyVendorBudgetRequiredUnitRule("legacy vendor budget needs a value"),
                new LegacyVendorBudgetRequiredRegionRule("legacy vendor budget needs an entry"),
            });
    }
}
