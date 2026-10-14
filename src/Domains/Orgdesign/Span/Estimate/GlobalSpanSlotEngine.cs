namespace Acme.Workforce.Orgdesign.Span.Estimate;

/// <summary>One step of the global span slot calculation.</summary>
public interface IGlobalSpanSlotAdjuster
{
    string Describe();
    double Adjust(GlobalSpanSlot item, double valueValue);
}

public sealed record MinimumGlobalSpanSlotAdjuster(double Minimum) : IGlobalSpanSlotAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(GlobalSpanSlot item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record TieredGlobalSpanSlotAdjuster(List<double> Bounds, List<double> Rates) : IGlobalSpanSlotAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(GlobalSpanSlot item, double valueValue)
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

public sealed record ChannelGlobalSpanSlotAdjuster(Dictionary<string, double> Factors) : IGlobalSpanSlotAdjuster
{
    public string Describe()
    {
        return "by channel";
    }

    public double Adjust(GlobalSpanSlot item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the global span slot steps in order and keeps a trace of each.</summary>
public sealed class GlobalSpanSlotEngine(List<IGlobalSpanSlotAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IGlobalSpanSlotAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static GlobalSpanSlotEngine DefaultGlobalSpanSlotEngine()
    {
        return new GlobalSpanSlotEngine(
            new List<IGlobalSpanSlotAdjuster>
            {
                new MinimumGlobalSpanSlotAdjuster(48.2),
                new TieredGlobalSpanSlotAdjuster(
                    new List<double> { 1298.0, 1321.0, 2957.0, 9254.0 },
                    new List<double> { 0.85, 1.12, 1.1, 0.85 }),
                new ChannelGlobalSpanSlotAdjuster(new Dictionary<string, double> { ["east"] = 1.25 }),
            },
            new List<string>(),
            3.4,
            99550.5);
    }

    public double Evaluate(GlobalSpanSlot item)
    {
        var current = item.WeightedTotal();
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

    public Dictionary<string, double> EvaluateAll(List<GlobalSpanSlot> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
