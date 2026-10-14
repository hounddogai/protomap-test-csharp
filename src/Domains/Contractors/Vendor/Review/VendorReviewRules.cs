namespace Acme.Workforce.Contractors.Vendor.Review;

/// <summary>A rule a vendor review must pass. Check returns its problems, or none.</summary>
public interface IVendorReviewRule
{
    List<string> Check(VendorReview item);
}

public sealed record VendorReviewCapacityRangeRule(int Min, int Max) : IVendorReviewRule
{
    public List<string> Check(VendorReview item)
    {
        var valueValue = item.Capacity;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"capacity {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"capacity {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record VendorReviewRequiredCodeRule(string Message) : IVendorReviewRule
{
    public List<string> Check(VendorReview item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record VendorReviewCodeLengthRule(int MaxLength) : IVendorReviewRule
{
    public List<string> Check(VendorReview item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record VendorReviewRequiredTitleRule(string Message) : IVendorReviewRule
{
    public List<string> Check(VendorReview item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record VendorReviewCodePrefixRule(string Prefix) : IVendorReviewRule
{
    public List<string> Check(VendorReview item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record VendorReviewRatingRangeRule(double Min, double Max) : IVendorReviewRule
{
    public List<string> Check(VendorReview item)
    {
        var valueValue = item.Rating;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"rating {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"rating {valueValue} is above {Max}");
        }
        return problems;
    }
}

/// <summary>Runs every rule of a vendor review.</summary>
public sealed record VendorReviewValidator(List<IVendorReviewRule> Rules)
{
    public List<string> Validate(VendorReview item)
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

    public void RequireValid(VendorReview item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<VendorReview> items)
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

/// <summary>Functions of vendor review rules.</summary>
public static class VendorReviewRules
{
    public static VendorReviewValidator DefaultVendorReviewValidator()
    {
        return new VendorReviewValidator(
            new List<IVendorReviewRule>
            {
                new VendorReviewCapacityRangeRule(32, 86400),
                new VendorReviewRequiredCodeRule("vendor review needs an entry"),
                new VendorReviewCodeLengthRule(1000),
                new VendorReviewRequiredTitleRule("vendor review needs a value"),
                new VendorReviewCodePrefixRule("REF"),
                new VendorReviewRatingRangeRule(3.7, 74.9),
            });
    }
}
