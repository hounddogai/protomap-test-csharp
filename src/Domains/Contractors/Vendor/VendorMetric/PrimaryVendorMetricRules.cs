namespace Acme.Workforce.Contractors.Vendor.VendorMetric;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a primary vendor metric must pass. Check returns its problems, or none.</summary>
public interface IPrimaryVendorMetricRule
{
    List<string> Check(PrimaryVendorMetric item);
}

public sealed record PrimaryVendorMetricDepartmentLengthRule(int MaxLength) : IPrimaryVendorMetricRule
{
    public List<string> Check(PrimaryVendorMetric item)
    {
        var size = item.Department.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"department is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PrimaryVendorMetricRequiredSummaryRule(string Message) : IPrimaryVendorMetricRule
{
    public List<string> Check(PrimaryVendorMetric item)
    {
        if (item.Summary.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PrimaryVendorMetricAllowedLabelRule(List<string> Allowed) : IPrimaryVendorMetricRule
{
    public List<string> Check(PrimaryVendorMetric item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Label)))
        {
            return new List<string> { $"label {item.Label} is duplicated" };
        }
        return new List<string>();
    }
}

public sealed record PrimaryVendorMetricRequiredCodeRule(string Message) : IPrimaryVendorMetricRule
{
    public List<string> Check(PrimaryVendorMetric item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PrimaryVendorMetricCodePrefixRule(string Prefix) : IPrimaryVendorMetricRule
{
    public List<string> Check(PrimaryVendorMetric item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a primary vendor metric.</summary>
public sealed record PrimaryVendorMetricValidator(List<IPrimaryVendorMetricRule> Rules)
{
    public List<string> Validate(PrimaryVendorMetric item)
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

    public void RequireValid(PrimaryVendorMetric item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PrimaryVendorMetric> items)
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

/// <summary>Functions of primary vendor metric rules.</summary>
public static class PrimaryVendorMetricRules
{
    public static PrimaryVendorMetricValidator DefaultPrimaryVendorMetricValidator()
    {
        return new PrimaryVendorMetricValidator(
            new List<IPrimaryVendorMetricRule>
            {
                new PrimaryVendorMetricDepartmentLengthRule(255),
                new PrimaryVendorMetricRequiredSummaryRule("primary vendor metric needs a value"),
                new PrimaryVendorMetricAllowedLabelRule(new List<string> { "primary", "default", "legacy" }),
                new PrimaryVendorMetricRequiredCodeRule("primary vendor metric needs this field"),
                new PrimaryVendorMetricCodePrefixRule("REF"),
            });
    }
}
