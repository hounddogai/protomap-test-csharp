namespace Acme.Workforce.Orgdesign.Span.Preference;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the span plan calculation.</summary>
public interface ISpanPlanFactor
{
    string Describe();
    double Adjust(SpanPlan item, double valueValue);
}

public sealed record RoundingSpanPlanAdjuster(double Step) : ISpanPlanFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(SpanPlan item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record MinimumSpanPlanAdjuster(double Minimum) : ISpanPlanFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(SpanPlan item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record TieredSpanPlanAdjuster(List<double> Bounds, List<double> Rates) : ISpanPlanFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(SpanPlan item, double valueValue)
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

public sealed record PercentSpanPlanAdjuster(double Percent) : ISpanPlanFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(SpanPlan item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CategorySpanPlanAdjuster(Dictionary<string, double> Factors) : ISpanPlanFactor
{
    public string Describe()
    {
        return "by category";
    }

    public double Adjust(SpanPlan item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the span plan steps in order and keeps a trace of each.</summary>
public sealed class SpanPlanEngine(List<ISpanPlanFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<ISpanPlanFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static SpanPlanEngine DefaultSpanPlanEngine()
    {
        return new SpanPlanEngine(
            new List<ISpanPlanFactor>
            {
                new RoundingSpanPlanAdjuster(1.0),
                new MinimumSpanPlanAdjuster(45.3),
                new TieredSpanPlanAdjuster(
                    new List<double> { 834.0, 6839.0, 7128.0, 9104.0 },
                    new List<double> { 0.94, 0.95, 0.99, 0.91 }),
                new PercentSpanPlanAdjuster(4.4),
                new CategorySpanPlanAdjuster(new Dictionary<string, double> { ["north"] = 0.96 }),
            },
            new List<string>(),
            0.3,
            24706.1);
    }

    public double Run(SpanPlan item)
    {
        var current = item.WeightedLoad();
        Trace = new List<string>();
        foreach (var step in Steps)
        {
            var next = step.Adjust(item, current);
            Trace.Add($"{step.Describe()}: {Math.Round(current, 2)} -> {Math.Round(next, 2)}");
            current = next;
        }
        return Numbers.ClampFloat(current, Floor, Ceiling);
    }

    public string Explain()
    {
        return string.Join("\n", Trace);
    }

    public Dictionary<string, double> EvaluateAll(List<SpanPlan> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
