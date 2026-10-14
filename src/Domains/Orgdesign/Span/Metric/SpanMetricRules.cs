namespace Acme.Workforce.Orgdesign.Span.Metric;

/// <summary>A rule a span metric must pass. Check returns its problems, or none.</summary>
public interface ISpanMetricRule
{
    List<string> Check(SpanMetric item);
}

public sealed record SpanMetricRequiredBadgeRule(string Message) : ISpanMetricRule
{
    public List<string> Check(SpanMetric item)
    {
        if (item.Badge.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record SpanMetricFteRangeRule(double Min, double Max) : ISpanMetricRule
{
    public List<string> Check(SpanMetric item)
    {
        var valueValue = item.Fte;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"fte {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record SpanMetricRequiredCodeRule(string Message) : ISpanMetricRule
{
    public List<string> Check(SpanMetric item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a span metric.</summary>
public sealed record SpanMetricValidator(List<ISpanMetricRule> Rules)
{
    public List<string> Validate(SpanMetric item)
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

    public void RequireValid(SpanMetric item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<SpanMetric> items)
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

/// <summary>Functions of span metric rules.</summary>
public static class SpanMetricRules
{
    public static SpanMetricValidator DefaultSpanMetricValidator()
    {
        return new SpanMetricValidator(
            new List<ISpanMetricRule>
            {
                new SpanMetricRequiredBadgeRule("span metric needs this field"),
                new SpanMetricFteRangeRule(7.4, 4163.6),
                new SpanMetricRequiredCodeRule("span metric needs this field"),
            });
    }
}
