namespace Acme.Workforce.Absence.ReturnPlan.Threshold;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a manual return plan metric must pass. Check returns its problems, or none.</summary>
public interface IManualReturnPlanMetricRule
{
    List<string> Check(ManualReturnPlanMetric item);
}

public sealed record ManualReturnPlanMetricCodeLengthRule(int MaxLength) : IManualReturnPlanMetricRule
{
    public List<string> Check(ManualReturnPlanMetric item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ManualReturnPlanMetricCodePrefixRule(string Prefix) : IManualReturnPlanMetricRule
{
    public List<string> Check(ManualReturnPlanMetric item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record ManualReturnPlanMetricRequiredSegmentRule(string Message) : IManualReturnPlanMetricRule
{
    public List<string> Check(ManualReturnPlanMetric item)
    {
        if (item.Segment.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ManualReturnPlanMetricDiscountRangeRule(double Min, double Max) : IManualReturnPlanMetricRule
{
    public List<string> Check(ManualReturnPlanMetric item)
    {
        var valueValue = item.Discount;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"discount {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"discount {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record ManualReturnPlanMetricAllowedOwnerIdRule(List<string> Allowed) : IManualReturnPlanMetricRule
{
    public List<string> Check(ManualReturnPlanMetric item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.OwnerId)))
        {
            return new List<string> { $"owner id {item.OwnerId} is not allowed" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a manual return plan metric.</summary>
public sealed record ManualReturnPlanMetricValidator(List<IManualReturnPlanMetricRule> Rules)
{
    public List<string> Validate(ManualReturnPlanMetric item)
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

    public void RequireValid(ManualReturnPlanMetric item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ManualReturnPlanMetric> items)
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

/// <summary>Functions of manual return plan metric rules.</summary>
public static class ManualReturnPlanMetricRules
{
    public static ManualReturnPlanMetricValidator DefaultManualReturnPlanMetricValidator()
    {
        return new ManualReturnPlanMetricValidator(
            new List<IManualReturnPlanMetricRule>
            {
                new ManualReturnPlanMetricCodeLengthRule(1000),
                new ManualReturnPlanMetricCodePrefixRule("ACM"),
                new ManualReturnPlanMetricRequiredSegmentRule("manual return plan metric needs a value"),
                new ManualReturnPlanMetricDiscountRangeRule(5.7, 2332.9),
                new ManualReturnPlanMetricAllowedOwnerIdRule(new List<string> { "capped" }),
            });
    }
}
