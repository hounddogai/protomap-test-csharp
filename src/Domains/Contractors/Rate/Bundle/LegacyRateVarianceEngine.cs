namespace Acme.Workforce.Contractors.Rate.Bundle;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the legacy rate variance calculation.</summary>
public interface ILegacyRateVarianceAdjuster
{
    string Describe();
    double Adjust(LegacyRateVariance item, double valueValue);
}

public sealed record TieredLegacyRateVarianceAdjuster(List<double> Bounds, List<double> Rates) : ILegacyRateVarianceAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(LegacyRateVariance item, double valueValue)
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

public sealed record PercentLegacyRateVarianceAdjuster(double Percent) : ILegacyRateVarianceAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(LegacyRateVariance item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record NameLegacyRateVarianceAdjuster(Dictionary<string, double> Factors) : ILegacyRateVarianceAdjuster
{
    public string Describe()
    {
        return "by name";
    }

    public double Adjust(LegacyRateVariance item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record FlaggedLegacyRateVarianceAdjuster(Registry Flags, string Flag, double Percent) : ILegacyRateVarianceAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(LegacyRateVariance item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the legacy rate variance steps in order and keeps a trace of each.</summary>
public sealed class LegacyRateVarianceEngine(List<ILegacyRateVarianceAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<ILegacyRateVarianceAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static LegacyRateVarianceEngine DefaultLegacyRateVarianceEngine(Registry flags)
    {
        return new LegacyRateVarianceEngine(
            new List<ILegacyRateVarianceAdjuster>
            {
                new TieredLegacyRateVarianceAdjuster(
                    new List<double> { 184.0, 2726.0 },
                    new List<double> { 0.91, 1.03 }),
                new PercentLegacyRateVarianceAdjuster(6.3),
                new NameLegacyRateVarianceAdjuster(
                    new Dictionary<string, double> { ["retail"] = 0.94, ["online"] = 1.31, ["central"] = 1.39 }),
                new FlaggedLegacyRateVarianceAdjuster(flags, "legacy-rate-variance-primary", -6.3),
            },
            new List<string>(),
            4.4,
            42184.4);
    }

    public double Compute(LegacyRateVariance item)
    {
        var current = item.ProjectedLoad();
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

    public Dictionary<string, double> EvaluateAll(List<LegacyRateVariance> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
