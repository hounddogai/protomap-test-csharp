namespace Acme.Workforce.Assets.Laptop.Metric;

/// <summary>A rule a lenient laptop metric must pass. Check returns its problems, or none.</summary>
public interface ILenientLaptopMetricRule
{
    List<string> Check(LenientLaptopMetric item);
}

public sealed record LenientLaptopMetricRequiredEmployeeIdRule(string Message) : ILenientLaptopMetricRule
{
    public List<string> Check(LenientLaptopMetric item)
    {
        if (item.EmployeeId.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record LenientLaptopMetricRequiredNameRule(string Message) : ILenientLaptopMetricRule
{
    public List<string> Check(LenientLaptopMetric item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record LenientLaptopMetricRequiredRegionRule(string Message) : ILenientLaptopMetricRule
{
    public List<string> Check(LenientLaptopMetric item)
    {
        if (item.Region.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record LenientLaptopMetricRequiredCodeRule(string Message) : ILenientLaptopMetricRule
{
    public List<string> Check(LenientLaptopMetric item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record LenientLaptopMetricNameLengthRule(int MaxLength) : ILenientLaptopMetricRule
{
    public List<string> Check(LenientLaptopMetric item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a lenient laptop metric.</summary>
public sealed record LenientLaptopMetricValidator(List<ILenientLaptopMetricRule> Rules)
{
    public List<string> Validate(LenientLaptopMetric item)
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

    public void RequireValid(LenientLaptopMetric item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<LenientLaptopMetric> items)
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

/// <summary>Functions of lenient laptop metric rules.</summary>
public static class LenientLaptopMetricRules
{
    public static LenientLaptopMetricValidator DefaultLenientLaptopMetricValidator()
    {
        return new LenientLaptopMetricValidator(
            new List<ILenientLaptopMetricRule>
            {
                new LenientLaptopMetricRequiredEmployeeIdRule("lenient laptop metric needs this field"),
                new LenientLaptopMetricRequiredNameRule("lenient laptop metric needs an entry"),
                new LenientLaptopMetricRequiredRegionRule("lenient laptop metric needs a value"),
                new LenientLaptopMetricRequiredCodeRule("lenient laptop metric needs an entry"),
                new LenientLaptopMetricNameLengthRule(1000),
            });
    }
}
