namespace Acme.Workforce.Contractors.Vendor.Schedule;

/// <summary>A rule a vendor record must pass. Check returns its problems, or none.</summary>
public interface IVendorRecordRule
{
    List<string> Check(VendorRecord item);
}

public sealed record VendorRecordRequiredCurrencyRule(string Message) : IVendorRecordRule
{
    public List<string> Check(VendorRecord item)
    {
        if (item.Currency.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record VendorRecordCountryLengthRule(int MaxLength) : IVendorRecordRule
{
    public List<string> Check(VendorRecord item)
    {
        var size = item.Country.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"country is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record VendorRecordRequiredCodeRule(string Message) : IVendorRecordRule
{
    public List<string> Check(VendorRecord item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record VendorRecordRequiredCountryRule(string Message) : IVendorRecordRule
{
    public List<string> Check(VendorRecord item)
    {
        if (item.Country.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a vendor record.</summary>
public sealed record VendorRecordValidator(List<IVendorRecordRule> Rules)
{
    public List<string> Validate(VendorRecord item)
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

    public void RequireValid(VendorRecord item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<VendorRecord> items)
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

/// <summary>Functions of vendor record rules.</summary>
public static class VendorRecordRules
{
    public static VendorRecordValidator DefaultVendorRecordValidator()
    {
        return new VendorRecordValidator(
            new List<IVendorRecordRule>
            {
                new VendorRecordRequiredCurrencyRule("vendor record needs this field"),
                new VendorRecordCountryLengthRule(80),
                new VendorRecordRequiredCodeRule("vendor record needs an entry"),
                new VendorRecordRequiredCountryRule("vendor record needs a value"),
            });
    }
}
