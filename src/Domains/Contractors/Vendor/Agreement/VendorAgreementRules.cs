namespace Acme.Workforce.Contractors.Vendor.Agreement;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a vendor agreement must pass. Check returns its problems, or none.</summary>
public interface IVendorAgreementRule
{
    List<string> Check(VendorAgreement item);
}

public sealed record VendorAgreementRequiredNameRule(string Message) : IVendorAgreementRule
{
    public List<string> Check(VendorAgreement item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record VendorAgreementAllowedCountryRule(List<string> Allowed) : IVendorAgreementRule
{
    public List<string> Check(VendorAgreement item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Country)))
        {
            return new List<string> { $"country {item.Country} is not allowed" };
        }
        return new List<string>();
    }
}

public sealed record VendorAgreementCodeLengthRule(int MaxLength) : IVendorAgreementRule
{
    public List<string> Check(VendorAgreement item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record VendorAgreementMaximumRangeRule(double Min, double Max) : IVendorAgreementRule
{
    public List<string> Check(VendorAgreement item)
    {
        var valueValue = item.Maximum;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"maximum {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a vendor agreement.</summary>
public sealed record VendorAgreementValidator(List<IVendorAgreementRule> Rules)
{
    public List<string> Validate(VendorAgreement item)
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

    public void RequireValid(VendorAgreement item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<VendorAgreement> items)
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

/// <summary>Functions of vendor agreement rules.</summary>
public static class VendorAgreementRules
{
    public static VendorAgreementValidator DefaultVendorAgreementValidator()
    {
        return new VendorAgreementValidator(
            new List<IVendorAgreementRule>
            {
                new VendorAgreementRequiredNameRule("vendor agreement needs a value"),
                new VendorAgreementAllowedCountryRule(new List<string> { "pending" }),
                new VendorAgreementCodeLengthRule(120),
                new VendorAgreementMaximumRangeRule(8.2, 4593.5),
            });
    }
}
