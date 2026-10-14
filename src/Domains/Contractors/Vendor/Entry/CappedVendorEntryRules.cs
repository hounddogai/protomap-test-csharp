namespace Acme.Workforce.Contractors.Vendor.Entry;

/// <summary>A rule a capped vendor entry must pass. Check returns its problems, or none.</summary>
public interface ICappedVendorEntryRule
{
    List<string> Check(CappedVendorEntry item);
}

public sealed record CappedVendorEntryRequiredCodeRule(string Message) : ICappedVendorEntryRule
{
    public List<string> Check(CappedVendorEntry item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record CappedVendorEntryCostRangeRule(double Min, double Max) : ICappedVendorEntryRule
{
    public List<string> Check(CappedVendorEntry item)
    {
        var valueValue = item.Cost;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"cost {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"cost {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record CappedVendorEntryLocaleLengthRule(int MaxLength) : ICappedVendorEntryRule
{
    public List<string> Check(CappedVendorEntry item)
    {
        var size = item.Locale.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"locale is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record CappedVendorEntryRequiredContactEmailRule(string Message) : ICappedVendorEntryRule
{
    public List<string> Check(CappedVendorEntry item)
    {
        if (item.ContactEmail.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a capped vendor entry.</summary>
public sealed record CappedVendorEntryValidator(List<ICappedVendorEntryRule> Rules)
{
    public List<string> Validate(CappedVendorEntry item)
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

    public void RequireValid(CappedVendorEntry item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<CappedVendorEntry> items)
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

/// <summary>Functions of capped vendor entry rules.</summary>
public static class CappedVendorEntryRules
{
    public static CappedVendorEntryValidator DefaultCappedVendorEntryValidator()
    {
        return new CappedVendorEntryValidator(
            new List<ICappedVendorEntryRule>
            {
                new CappedVendorEntryRequiredCodeRule("capped vendor entry needs a value"),
                new CappedVendorEntryCostRangeRule(1.9, 3330.9),
                new CappedVendorEntryLocaleLengthRule(80),
                new CappedVendorEntryRequiredContactEmailRule("capped vendor entry needs this field"),
            });
    }
}
