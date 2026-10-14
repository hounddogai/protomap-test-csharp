namespace Acme.Workforce.Orgdesign.Span.Baseline;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the span rule calculation.</summary>
public interface ISpanRuleStep
{
    string Describe();
    double Adjust(SpanRule item, double valueValue);
}

public sealed record DepartmentSpanRuleAdjuster(Dictionary<string, double> Factors) : ISpanRuleStep
{
    public string Describe()
    {
        return "by department";
    }

    public double Adjust(SpanRule item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record SpanRuleAttemptsOffsetAdjuster(double Weight) : ISpanRuleStep
{
    public string Describe()
    {
        return $"plus attempts times {Weight}";
    }

    public double Adjust(SpanRule item, double valueValue)
    {
        return valueValue + (double)item.Attempts * Weight;
    }
}

public sealed record PercentSpanRuleAdjuster(double Percent) : ISpanRuleStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(SpanRule item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredSpanRuleAdjuster(List<double> Bounds, List<double> Rates) : ISpanRuleStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(SpanRule item, double valueValue)
    {
        for (var index = 0; index < Bounds.Count; index++)
        {
            if (valueValue <= Bounds[index] && index < Rates.Count)
            {
                return valueValue * Rates[index];
            }
        }
        return valueValue;
    }
}

public sealed record FlaggedSpanRuleAdjuster(Registry Flags, string Flag, double Percent) : ISpanRuleStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(SpanRule item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the span rule steps in order and keeps a trace of each.</summary>
public sealed class SpanRuleEngine(List<ISpanRuleStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<ISpanRuleStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static SpanRuleEngine DefaultSpanRuleEngine(Registry flags)
    {
        return new SpanRuleEngine(
            new List<ISpanRuleStep>
            {
                new DepartmentSpanRuleAdjuster(
                    new Dictionary<string, double> { ["online"] = 1.04, ["east"] = 0.77, ["west"] = 0.88 }),
                new SpanRuleAttemptsOffsetAdjuster(1.19),
                new PercentSpanRuleAdjuster(-2.2),
                new TieredSpanRuleAdjuster(
                    new List<double> { 2654.0, 6619.0, 8325.0 },
                    new List<double> { 1.01, 1.03, 1.01 }),
                new FlaggedSpanRuleAdjuster(flags, "span-rule-scheduled", 9.3),
            },
            new List<string>(),
            3.9,
            15649.9);
    }

    public double Run(SpanRule item)
    {
        var current = item.MeasuredValue();
        Trace = new List<string>();
        for (var index = 0; index < Steps.Count; index++)
        {
            var step = Steps[index];
            var before = current;
            current = step.Adjust(item, current);
            if (before != current)
            {
                Trace.Add($"{index + 1}. {step.Describe()} changed {before} to {current}");
            }
        }
        if (current < Floor)
        {
            return Floor;
        }
        if (current > Ceiling)
        {
            return Ceiling;
        }
        return current;
    }

    public string Explain()
    {
        return string.Join("\n", Trace);
    }

    public Dictionary<string, double> EvaluateAll(List<SpanRule> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
