namespace Acme.Workforce.Compliance.Certification.CertificationSummary;

/// <summary>A rule a primary certification bundle must pass. Check returns its problems, or none.</summary>
public interface IPrimaryCertificationBundleRule
{
    List<string> Check(PrimaryCertificationBundle item);
}

public sealed record PrimaryCertificationBundleNameLengthRule(int MaxLength) : IPrimaryCertificationBundleRule
{
    public List<string> Check(PrimaryCertificationBundle item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PrimaryCertificationBundleCountryLengthRule(int MaxLength) : IPrimaryCertificationBundleRule
{
    public List<string> Check(PrimaryCertificationBundle item)
    {
        var size = item.Country.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"country is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PrimaryCertificationBundleRequiredCategoryRule(string Message) : IPrimaryCertificationBundleRule
{
    public List<string> Check(PrimaryCertificationBundle item)
    {
        if (item.Category.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PrimaryCertificationBundleCodePrefixRule(string Prefix) : IPrimaryCertificationBundleRule
{
    public List<string> Check(PrimaryCertificationBundle item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a primary certification bundle.</summary>
public sealed record PrimaryCertificationBundleValidator(List<IPrimaryCertificationBundleRule> Rules)
{
    public List<string> Validate(PrimaryCertificationBundle item)
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

    public void RequireValid(PrimaryCertificationBundle item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PrimaryCertificationBundle> items)
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

/// <summary>Functions of primary certification bundle rules.</summary>
public static class PrimaryCertificationBundleRules
{
    public static PrimaryCertificationBundleValidator DefaultPrimaryCertificationBundleValidator()
    {
        return new PrimaryCertificationBundleValidator(
            new List<IPrimaryCertificationBundleRule>
            {
                new PrimaryCertificationBundleNameLengthRule(80),
                new PrimaryCertificationBundleCountryLengthRule(1000),
                new PrimaryCertificationBundleRequiredCategoryRule("primary certification bundle needs a value"),
                new PrimaryCertificationBundleCodePrefixRule("REF"),
            });
    }
}
