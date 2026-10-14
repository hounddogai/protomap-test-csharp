namespace Acme.Workforce.Assets.LicenseSeat.Mapping;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the license seat metric calculation.</summary>
public interface ILicenseSeatMetricFactor
{
    string Describe();
    double Adjust(LicenseSeatMetric item, double valueValue);
}

public sealed record FlaggedLicenseSeatMetricAdjuster(Registry Flags, string Flag, double Percent) : ILicenseSeatMetricFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(LicenseSeatMetric item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingLicenseSeatMetricAdjuster(double Step) : ILicenseSeatMetricFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(LicenseSeatMetric item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record CapLicenseSeatMetricAdjuster(double Cap) : ILicenseSeatMetricFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(LicenseSeatMetric item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record PercentLicenseSeatMetricAdjuster(double Percent) : ILicenseSeatMetricFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(LicenseSeatMetric item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredLicenseSeatMetricAdjuster(List<double> Bounds, List<double> Rates) : ILicenseSeatMetricFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(LicenseSeatMetric item, double valueValue)
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

/// <summary>Runs the license seat metric steps in order and keeps a trace of each.</summary>
public sealed class LicenseSeatMetricEngine(List<ILicenseSeatMetricFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<ILicenseSeatMetricFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static LicenseSeatMetricEngine DefaultLicenseSeatMetricEngine(Registry flags)
    {
        return new LicenseSeatMetricEngine(
            new List<ILicenseSeatMetricFactor>
            {
                new FlaggedLicenseSeatMetricAdjuster(flags, "license-seat-metric-adaptive", -8.9),
                new RoundingLicenseSeatMetricAdjuster(0.1),
                new CapLicenseSeatMetricAdjuster(48967.0),
                new PercentLicenseSeatMetricAdjuster(-4.8),
                new TieredLicenseSeatMetricAdjuster(
                    new List<double> { 280.0, 6892.0 },
                    new List<double> { 0.85, 1.17 }),
            },
            new List<string>(),
            0.2,
            10283.4);
    }

    public double Evaluate(LicenseSeatMetric item)
    {
        var current = item.ProjectedTotal();
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

    public Dictionary<string, double> EvaluateAll(List<LicenseSeatMetric> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
