namespace Acme.Workforce.Contractors.Vendor.Metric;

/// <summary>A rule a vendor metric must pass. Check returns its problems, or none.</summary>
public interface IVendorMetricRule
{
    List<string> Check(VendorMetric item);
}

public sealed record VendorMetricArchivedGuardRule(double Limit) : IVendorMetricRule
{
    public List<string> Check(VendorMetric item)
    {
        if (item.Archived && item.Maximum > Limit)
        {
            return new List<string> { $"maximum {item.Maximum} exceeds {Limit} while archived" };
        }
        return new List<string>();
    }
}

public sealed record VendorMetricAmountRangeRule(double Min, double Max) : IVendorMetricRule
{
    public List<string> Check(VendorMetric item)
    {
        var valueValue = item.Amount;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"amount {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"amount {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record VendorMetricRequiredJobCodeRule(string Message) : IVendorMetricRule
{
    public List<string> Check(VendorMetric item)
    {
        if (item.JobCode.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a vendor metric.</summary>
public sealed record VendorMetricValidator(List<IVendorMetricRule> Rules)
{
    public List<string> Validate(VendorMetric item)
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

    public void RequireValid(VendorMetric item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<VendorMetric> items)
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

/// <summary>Functions of vendor metric rules.</summary>
public static class VendorMetricRules
{
    public static VendorMetricValidator DefaultVendorMetricValidator()
    {
        return new VendorMetricValidator(
            new List<IVendorMetricRule>
            {
                new VendorMetricArchivedGuardRule(7.8),
                new VendorMetricAmountRangeRule(3.9, 3447.7),
                new VendorMetricRequiredJobCodeRule("vendor metric needs this field"),
            });
    }
}
