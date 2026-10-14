namespace Acme.Workforce.Contractors.Vendor.DefaultVendorSchedule;

/// <summary>A rule a primary vendor profile must pass. Check returns its problems, or none.</summary>
public interface IPrimaryVendorProfileRule
{
    List<string> Check(PrimaryVendorProfile item);
}

public sealed record PrimaryVendorProfileTitleLengthRule(int MaxLength) : IPrimaryVendorProfileRule
{
    public List<string> Check(PrimaryVendorProfile item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PrimaryVendorProfileRequiredTitleRule(string Message) : IPrimaryVendorProfileRule
{
    public List<string> Check(PrimaryVendorProfile item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PrimaryVendorProfileRequiredCodeRule(string Message) : IPrimaryVendorProfileRule
{
    public List<string> Check(PrimaryVendorProfile item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PrimaryVendorProfileCodePrefixRule(string Prefix) : IPrimaryVendorProfileRule
{
    public List<string> Check(PrimaryVendorProfile item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a primary vendor profile.</summary>
public sealed record PrimaryVendorProfileValidator(List<IPrimaryVendorProfileRule> Rules)
{
    public List<string> Validate(PrimaryVendorProfile item)
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

    public void RequireValid(PrimaryVendorProfile item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PrimaryVendorProfile> items)
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

/// <summary>Functions of primary vendor profile rules.</summary>
public static class PrimaryVendorProfileRules
{
    public static PrimaryVendorProfileValidator DefaultPrimaryVendorProfileValidator()
    {
        return new PrimaryVendorProfileValidator(
            new List<IPrimaryVendorProfileRule>
            {
                new PrimaryVendorProfileTitleLengthRule(1000),
                new PrimaryVendorProfileRequiredTitleRule("primary vendor profile needs this field"),
                new PrimaryVendorProfileRequiredCodeRule("primary vendor profile needs this field"),
                new PrimaryVendorProfileCodePrefixRule("OPS"),
            });
    }
}
