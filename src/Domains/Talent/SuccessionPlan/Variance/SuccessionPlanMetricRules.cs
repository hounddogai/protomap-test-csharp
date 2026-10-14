namespace Acme.Workforce.Talent.SuccessionPlan.Variance;

/// <summary>A rule a succession plan metric must pass. Check returns its problems, or none.</summary>
public interface ISuccessionPlanMetricRule
{
    List<string> Check(SuccessionPlanMetric item);
}

public sealed record SuccessionPlanMetricCodeLengthRule(int MaxLength) : ISuccessionPlanMetricRule
{
    public List<string> Check(SuccessionPlanMetric item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record SuccessionPlanMetricRequiredCodeRule(string Message) : ISuccessionPlanMetricRule
{
    public List<string> Check(SuccessionPlanMetric item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record SuccessionPlanMetricDiscountRangeRule(double Min, double Max) : ISuccessionPlanMetricRule
{
    public List<string> Check(SuccessionPlanMetric item)
    {
        var valueValue = item.Discount;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"discount {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record SuccessionPlanMetricTitleLengthRule(int MaxLength) : ISuccessionPlanMetricRule
{
    public List<string> Check(SuccessionPlanMetric item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a succession plan metric.</summary>
public sealed record SuccessionPlanMetricValidator(List<ISuccessionPlanMetricRule> Rules)
{
    public List<string> Validate(SuccessionPlanMetric item)
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

    public void RequireValid(SuccessionPlanMetric item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<SuccessionPlanMetric> items)
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

/// <summary>Functions of succession plan metric rules.</summary>
public static class SuccessionPlanMetricRules
{
    public static SuccessionPlanMetricValidator DefaultSuccessionPlanMetricValidator()
    {
        return new SuccessionPlanMetricValidator(
            new List<ISuccessionPlanMetricRule>
            {
                new SuccessionPlanMetricCodeLengthRule(120),
                new SuccessionPlanMetricRequiredCodeRule("succession plan metric needs an entry"),
                new SuccessionPlanMetricDiscountRangeRule(7.6, 1682.0),
                new SuccessionPlanMetricTitleLengthRule(1000),
            });
    }
}
