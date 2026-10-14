namespace Acme.Workforce.Contractors.Engagement.Summary;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the engagement mapping calculation.</summary>
public interface IEngagementMappingAdjuster
{
    string Describe();
    double Adjust(EngagementMapping item, double valueValue);
}

public sealed record MinimumEngagementMappingAdjuster(double Minimum) : IEngagementMappingAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(EngagementMapping item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record TieredEngagementMappingAdjuster(List<double> Bounds, List<double> Rates) : IEngagementMappingAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(EngagementMapping item, double valueValue)
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

public sealed record CapEngagementMappingAdjuster(double Cap) : IEngagementMappingAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(EngagementMapping item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record RoundingEngagementMappingAdjuster(double Step) : IEngagementMappingAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(EngagementMapping item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record EngagementMappingMarginOffsetAdjuster(double Weight) : IEngagementMappingAdjuster
{
    public string Describe()
    {
        return $"plus margin times {Weight}";
    }

    public double Adjust(EngagementMapping item, double valueValue)
    {
        return valueValue + item.Margin * Weight;
    }
}

/// <summary>Runs the engagement mapping steps in order and keeps a trace of each.</summary>
public sealed class EngagementMappingEngine(List<IEngagementMappingAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IEngagementMappingAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static EngagementMappingEngine DefaultEngagementMappingEngine()
    {
        return new EngagementMappingEngine(
            new List<IEngagementMappingAdjuster>
            {
                new MinimumEngagementMappingAdjuster(21.5),
                new TieredEngagementMappingAdjuster(
                    new List<double> { 2881.0, 5502.0 },
                    new List<double> { 0.91, 0.92 }),
                new CapEngagementMappingAdjuster(28443.0),
                new RoundingEngagementMappingAdjuster(1.0),
                new EngagementMappingMarginOffsetAdjuster(1.42),
            },
            new List<string>(),
            0.8,
            11250.3);
    }

    public double Price(EngagementMapping item)
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

    public Dictionary<string, double> EvaluateAll(List<EngagementMapping> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
