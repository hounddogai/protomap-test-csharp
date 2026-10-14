namespace Acme.Workforce.Orgdesign.Span.Mapping;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the span mapping calculation.</summary>
public interface ISpanMappingAdjuster
{
    string Describe();
    double Adjust(SpanMapping item, double valueValue);
}

public sealed record TeamSpanMappingAdjuster(Dictionary<string, double> Factors) : ISpanMappingAdjuster
{
    public string Describe()
    {
        return "by team";
    }

    public double Adjust(SpanMapping item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record MinimumSpanMappingAdjuster(double Minimum) : ISpanMappingAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(SpanMapping item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record TieredSpanMappingAdjuster(List<double> Bounds, List<double> Rates) : ISpanMappingAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(SpanMapping item, double valueValue)
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

public sealed record PercentSpanMappingAdjuster(double Percent) : ISpanMappingAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(SpanMapping item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapSpanMappingAdjuster(double Cap) : ISpanMappingAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(SpanMapping item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

/// <summary>Runs the span mapping steps in order and keeps a trace of each.</summary>
public sealed class SpanMappingEngine(List<ISpanMappingAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<ISpanMappingAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static SpanMappingEngine DefaultSpanMappingEngine()
    {
        return new SpanMappingEngine(
            new List<ISpanMappingAdjuster>
            {
                new TeamSpanMappingAdjuster(
                    new Dictionary<string, double> { ["online"] = 1.23, ["east"] = 0.87, ["west"] = 1.3 }),
                new MinimumSpanMappingAdjuster(28.3),
                new TieredSpanMappingAdjuster(new List<double> { 2786.0, 6831.0 }, new List<double> { 1.15, 1.19 }),
                new PercentSpanMappingAdjuster(11.6),
                new CapSpanMappingAdjuster(22321.0),
            },
            new List<string>(),
            2.3,
            8586.5);
    }

    public double Run(SpanMapping item)
    {
        var current = item.MeasuredValue();
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

    public Dictionary<string, double> EvaluateAll(List<SpanMapping> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
