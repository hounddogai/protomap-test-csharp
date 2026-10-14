namespace Acme.Workforce.Contractors.Vendor.Schedule;

/// <summary>A rule a vendor entry must pass. Check returns its problems, or none.</summary>
public interface IVendorEntryRule
{
    List<string> Check(VendorEntry item);
}

public sealed record VendorEntryLeaveDaysRangeRule(int Min, int Max) : IVendorEntryRule
{
    public List<string> Check(VendorEntry item)
    {
        var valueValue = item.LeaveDays;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"leave days {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"leave days {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record VendorEntryRequiredExternalRefRule(string Message) : IVendorEntryRule
{
    public List<string> Check(VendorEntry item)
    {
        if (item.ExternalRef.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record VendorEntryRequiredGradeRule(string Message) : IVendorEntryRule
{
    public List<string> Check(VendorEntry item)
    {
        if (item.Grade.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a vendor entry.</summary>
public sealed record VendorEntryValidator(List<IVendorEntryRule> Rules)
{
    public List<string> Validate(VendorEntry item)
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

    public void RequireValid(VendorEntry item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<VendorEntry> items)
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

/// <summary>Functions of vendor entry rules.</summary>
public static class VendorEntryRules
{
    public static VendorEntryValidator DefaultVendorEntryValidator()
    {
        return new VendorEntryValidator(
            new List<IVendorEntryRule>
            {
                new VendorEntryLeaveDaysRangeRule(120, 3600),
                new VendorEntryRequiredExternalRefRule("vendor entry needs a value"),
                new VendorEntryRequiredGradeRule("vendor entry needs a value"),
            });
    }
}
