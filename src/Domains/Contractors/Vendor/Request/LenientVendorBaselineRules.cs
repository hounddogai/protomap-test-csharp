namespace Acme.Workforce.Contractors.Vendor.Request;

/// <summary>A rule a lenient vendor baseline must pass. Check returns its problems, or none.</summary>
public interface ILenientVendorBaselineRule
{
    List<string> Check(LenientVendorBaseline item);
}

public sealed record LenientVendorBaselineRequiredReviewerRule(string Message) : ILenientVendorBaselineRule
{
    public List<string> Check(LenientVendorBaseline item)
    {
        if (item.Reviewer.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record LenientVendorBaselineCodeLengthRule(int MaxLength) : ILenientVendorBaselineRule
{
    public List<string> Check(LenientVendorBaseline item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LenientVendorBaselineRequiredCodeRule(string Message) : ILenientVendorBaselineRule
{
    public List<string> Check(LenientVendorBaseline item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record LenientVendorBaselineVisibleGuardRule(int Limit) : ILenientVendorBaselineRule
{
    public List<string> Check(LenientVendorBaseline item)
    {
        if (item.Visible && item.Version > Limit)
        {
            return new List<string> { $"version {item.Version} exceeds {Limit} while visible" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a lenient vendor baseline.</summary>
public sealed record LenientVendorBaselineValidator(List<ILenientVendorBaselineRule> Rules)
{
    public List<string> Validate(LenientVendorBaseline item)
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

    public void RequireValid(LenientVendorBaseline item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<LenientVendorBaseline> items)
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

/// <summary>Functions of lenient vendor baseline rules.</summary>
public static class LenientVendorBaselineRules
{
    public static LenientVendorBaselineValidator DefaultLenientVendorBaselineValidator()
    {
        return new LenientVendorBaselineValidator(
            new List<ILenientVendorBaselineRule>
            {
                new LenientVendorBaselineRequiredReviewerRule("lenient vendor baseline needs this field"),
                new LenientVendorBaselineCodeLengthRule(80),
                new LenientVendorBaselineRequiredCodeRule("lenient vendor baseline needs an entry"),
                new LenientVendorBaselineVisibleGuardRule(32),
            });
    }
}
