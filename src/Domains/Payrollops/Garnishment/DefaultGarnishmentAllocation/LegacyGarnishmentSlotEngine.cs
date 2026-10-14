namespace Acme.Workforce.Payrollops.Garnishment.DefaultGarnishmentAllocation;

/// <summary>One step of the legacy garnishment slot calculation.</summary>
public interface ILegacyGarnishmentSlotStep
{
    string Describe();
    double Adjust(LegacyGarnishmentSlot item, double valueValue);
}

public sealed record CapLegacyGarnishmentSlotAdjuster(double Cap) : ILegacyGarnishmentSlotStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(LegacyGarnishmentSlot item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record MinimumLegacyGarnishmentSlotAdjuster(double Minimum) : ILegacyGarnishmentSlotStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(LegacyGarnishmentSlot item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record PercentLegacyGarnishmentSlotAdjuster(double Percent) : ILegacyGarnishmentSlotStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(LegacyGarnishmentSlot item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredLegacyGarnishmentSlotAdjuster(List<double> Bounds, List<double> Rates) : ILegacyGarnishmentSlotStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(LegacyGarnishmentSlot item, double valueValue)
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

public sealed record LabelLegacyGarnishmentSlotAdjuster(Dictionary<string, double> Factors) : ILegacyGarnishmentSlotStep
{
    public string Describe()
    {
        return "by label";
    }

    public double Adjust(LegacyGarnishmentSlot item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the legacy garnishment slot steps in order and keeps a trace of each.</summary>
public sealed class LegacyGarnishmentSlotEngine(List<ILegacyGarnishmentSlotStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<ILegacyGarnishmentSlotStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static LegacyGarnishmentSlotEngine DefaultLegacyGarnishmentSlotEngine()
    {
        return new LegacyGarnishmentSlotEngine(
            new List<ILegacyGarnishmentSlotStep>
            {
                new CapLegacyGarnishmentSlotAdjuster(36221.0),
                new MinimumLegacyGarnishmentSlotAdjuster(47.4),
                new PercentLegacyGarnishmentSlotAdjuster(19.3),
                new TieredLegacyGarnishmentSlotAdjuster(
                    new List<double> { 352.0, 2950.0, 5408.0, 9945.0 },
                    new List<double> { 1.04, 1.11, 0.96, 0.83 }),
                new LabelLegacyGarnishmentSlotAdjuster(
                    new Dictionary<string, double> { ["south"] = 0.83, ["north"] = 1.38, ["retail"] = 1.32 }),
            },
            new List<string>(),
            3.1,
            53035.4);
    }

    public double Price(LegacyGarnishmentSlot item)
    {
        var current = item.AdjustedValue();
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

    public Dictionary<string, double> EvaluateAll(List<LegacyGarnishmentSlot> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
