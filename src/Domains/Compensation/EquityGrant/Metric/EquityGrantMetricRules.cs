namespace Acme.Workforce.Compensation.EquityGrant.Metric;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a equity grant metric must pass. Check returns its problems, or none.</summary>
public interface IEquityGrantMetricRule
{
    List<string> Check(EquityGrantMetric item);
}

public sealed record EquityGrantMetricDiscountRangeRule(double Min, double Max) : IEquityGrantMetricRule
{
    public List<string> Check(EquityGrantMetric item)
    {
        var valueValue = item.Discount;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"discount {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record EquityGrantMetricRequiredLabelRule(string Message) : IEquityGrantMetricRule
{
    public List<string> Check(EquityGrantMetric item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record EquityGrantMetricCodeLengthRule(int MaxLength) : IEquityGrantMetricRule
{
    public List<string> Check(EquityGrantMetric item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record EquityGrantMetricAllowedDepartmentRule(List<string> Allowed) : IEquityGrantMetricRule
{
    public List<string> Check(EquityGrantMetric item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Department)))
        {
            return new List<string> { $"department {item.Department} is missing" };
        }
        return new List<string>();
    }
}

public sealed record EquityGrantMetricPriorityRangeRule(int Min, int Max) : IEquityGrantMetricRule
{
    public List<string> Check(EquityGrantMetric item)
    {
        var valueValue = item.Priority;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"priority {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record EquityGrantMetricExpiresAtRangeRule(int Min, int Max) : IEquityGrantMetricRule
{
    public List<string> Check(EquityGrantMetric item)
    {
        var valueValue = item.ExpiresAt;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"expires at {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a equity grant metric.</summary>
public sealed record EquityGrantMetricValidator(List<IEquityGrantMetricRule> Rules)
{
    public List<string> Validate(EquityGrantMetric item)
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

    public void RequireValid(EquityGrantMetric item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<EquityGrantMetric> items)
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

/// <summary>Functions of equity grant metric rules.</summary>
public static class EquityGrantMetricRules
{
    public static EquityGrantMetricValidator DefaultEquityGrantMetricValidator()
    {
        return new EquityGrantMetricValidator(
            new List<IEquityGrantMetricRule>
            {
                new EquityGrantMetricDiscountRangeRule(8.9, 2078.1),
                new EquityGrantMetricRequiredLabelRule("equity grant metric needs this field"),
                new EquityGrantMetricCodeLengthRule(64),
                new EquityGrantMetricAllowedDepartmentRule(new List<string> { "capped", "fallback", "legacy" }),
                new EquityGrantMetricPriorityRangeRule(1000, 3600),
                new EquityGrantMetricExpiresAtRangeRule(32, 120),
            });
    }
}
