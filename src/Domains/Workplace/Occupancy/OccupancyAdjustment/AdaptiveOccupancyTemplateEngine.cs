namespace Acme.Workforce.Workplace.Occupancy.OccupancyAdjustment;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the adaptive occupancy template calculation.</summary>
public interface IAdaptiveOccupancyTemplateModifier
{
    string Describe();
    double Adjust(AdaptiveOccupancyTemplate item, double valueValue);
}

public sealed record PercentAdaptiveOccupancyTemplateAdjuster(double Percent) : IAdaptiveOccupancyTemplateModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(AdaptiveOccupancyTemplate item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record SourceAdaptiveOccupancyTemplateAdjuster(Dictionary<string, double> Factors) : IAdaptiveOccupancyTemplateModifier
{
    public string Describe()
    {
        return "by source";
    }

    public double Adjust(AdaptiveOccupancyTemplate item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record CapAdaptiveOccupancyTemplateAdjuster(double Cap) : IAdaptiveOccupancyTemplateModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(AdaptiveOccupancyTemplate item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record RoundingAdaptiveOccupancyTemplateAdjuster(double Step) : IAdaptiveOccupancyTemplateModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(AdaptiveOccupancyTemplate item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the adaptive occupancy template steps in order and keeps a trace of each.</summary>
public sealed class AdaptiveOccupancyTemplateEngine(List<IAdaptiveOccupancyTemplateModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IAdaptiveOccupancyTemplateModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static AdaptiveOccupancyTemplateEngine DefaultAdaptiveOccupancyTemplateEngine()
    {
        return new AdaptiveOccupancyTemplateEngine(
            new List<IAdaptiveOccupancyTemplateModifier>
            {
                new PercentAdaptiveOccupancyTemplateAdjuster(13.2),
                new SourceAdaptiveOccupancyTemplateAdjuster(new Dictionary<string, double> { ["east"] = 0.92 }),
                new CapAdaptiveOccupancyTemplateAdjuster(32651.0),
                new RoundingAdaptiveOccupancyTemplateAdjuster(5.0),
            },
            new List<string>(),
            0.6,
            12982.3);
    }

    public double Evaluate(AdaptiveOccupancyTemplate item)
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

    public Dictionary<string, double> EvaluateAll(List<AdaptiveOccupancyTemplate> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
