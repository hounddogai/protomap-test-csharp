namespace Acme.Workforce.Contractors.Vendor.Rollup;

/// <summary>A rule a global vendor review must pass. Check returns its problems, or none.</summary>
public interface IGlobalVendorReviewRule
{
    List<string> Check(GlobalVendorReview item);
}

public sealed record GlobalVendorReviewRequiredCountryRule(string Message) : IGlobalVendorReviewRule
{
    public List<string> Check(GlobalVendorReview item)
    {
        if (item.Country.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record GlobalVendorReviewRequiredChannelRule(string Message) : IGlobalVendorReviewRule
{
    public List<string> Check(GlobalVendorReview item)
    {
        if (item.Channel.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record GlobalVendorReviewCodePrefixRule(string Prefix) : IGlobalVendorReviewRule
{
    public List<string> Check(GlobalVendorReview item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record GlobalVendorReviewRequiredCodeRule(string Message) : IGlobalVendorReviewRule
{
    public List<string> Check(GlobalVendorReview item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a global vendor review.</summary>
public sealed record GlobalVendorReviewValidator(List<IGlobalVendorReviewRule> Rules)
{
    public List<string> Validate(GlobalVendorReview item)
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

    public void RequireValid(GlobalVendorReview item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<GlobalVendorReview> items)
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

/// <summary>Functions of global vendor review rules.</summary>
public static class GlobalVendorReviewRules
{
    public static GlobalVendorReviewValidator DefaultGlobalVendorReviewValidator()
    {
        return new GlobalVendorReviewValidator(
            new List<IGlobalVendorReviewRule>
            {
                new GlobalVendorReviewRequiredCountryRule("global vendor review needs an entry"),
                new GlobalVendorReviewRequiredChannelRule("global vendor review needs a value"),
                new GlobalVendorReviewCodePrefixRule("ACM"),
                new GlobalVendorReviewRequiredCodeRule("global vendor review needs an entry"),
            });
    }
}
