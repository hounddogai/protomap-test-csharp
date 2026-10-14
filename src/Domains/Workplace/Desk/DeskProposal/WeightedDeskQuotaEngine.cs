namespace Acme.Workforce.Workplace.Desk.DeskProposal;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the weighted desk quota calculation.</summary>
public interface IWeightedDeskQuotaFactor
{
    string Describe();
    double Adjust(WeightedDeskQuota item, double valueValue);
}

public sealed record RoundingWeightedDeskQuotaAdjuster(double Step) : IWeightedDeskQuotaFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(WeightedDeskQuota item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record WeightedDeskQuotaBatchSizeOffsetAdjuster(double Weight) : IWeightedDeskQuotaFactor
{
    public string Describe()
    {
        return $"plus batch size times {Weight}";
    }

    public double Adjust(WeightedDeskQuota item, double valueValue)
    {
        return valueValue + (double)item.BatchSize * Weight;
    }
}

public sealed record MinimumWeightedDeskQuotaAdjuster(double Minimum) : IWeightedDeskQuotaFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(WeightedDeskQuota item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record LabelWeightedDeskQuotaAdjuster(Dictionary<string, double> Factors) : IWeightedDeskQuotaFactor
{
    public string Describe()
    {
        return "by label";
    }

    public double Adjust(WeightedDeskQuota item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record TieredWeightedDeskQuotaAdjuster(List<double> Bounds, List<double> Rates) : IWeightedDeskQuotaFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(WeightedDeskQuota item, double valueValue)
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

public sealed record FlaggedWeightedDeskQuotaAdjuster(Registry Flags, string Flag, double Percent) : IWeightedDeskQuotaFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(WeightedDeskQuota item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the weighted desk quota steps in order and keeps a trace of each.</summary>
public sealed class WeightedDeskQuotaEngine(List<IWeightedDeskQuotaFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IWeightedDeskQuotaFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static WeightedDeskQuotaEngine DefaultWeightedDeskQuotaEngine(Registry flags)
    {
        return new WeightedDeskQuotaEngine(
            new List<IWeightedDeskQuotaFactor>
            {
                new RoundingWeightedDeskQuotaAdjuster(0.25),
                new WeightedDeskQuotaBatchSizeOffsetAdjuster(1.49),
                new MinimumWeightedDeskQuotaAdjuster(43.6),
                new LabelWeightedDeskQuotaAdjuster(
                    new Dictionary<string, double> { ["retail"] = 1.4, ["west"] = 1.38 }),
                new TieredWeightedDeskQuotaAdjuster(
                    new List<double> { 237.0, 4713.0, 4900.0 },
                    new List<double> { 0.96, 0.98, 1.07 }),
                new FlaggedWeightedDeskQuotaAdjuster(flags, "weighted-desk-quota-regional", -6.0),
            },
            new List<string>(),
            4.0,
            96140.8);
    }

    public double Run(WeightedDeskQuota item)
    {
        var current = item.ProjectedVolume();
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

    public Dictionary<string, double> EvaluateAll(List<WeightedDeskQuota> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
