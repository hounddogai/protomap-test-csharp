namespace Acme.Workforce.Talent.Nomination.Target;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the nomination metric calculation.</summary>
public interface INominationMetricFactor
{
    string Describe();
    double Adjust(NominationMetric item, double valueValue);
}

public sealed record FlaggedNominationMetricAdjuster(Registry Flags, string Flag, double Percent) : INominationMetricFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(NominationMetric item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record PercentNominationMetricAdjuster(double Percent) : INominationMetricFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(NominationMetric item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record NameNominationMetricAdjuster(Dictionary<string, double> Factors) : INominationMetricFactor
{
    public string Describe()
    {
        return "by name";
    }

    public double Adjust(NominationMetric item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record TieredNominationMetricAdjuster(List<double> Bounds, List<double> Rates) : INominationMetricFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(NominationMetric item, double valueValue)
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

public sealed record NominationMetricCostOffsetAdjuster(double Weight) : INominationMetricFactor
{
    public string Describe()
    {
        return $"plus cost times {Weight}";
    }

    public double Adjust(NominationMetric item, double valueValue)
    {
        return valueValue + item.Cost * Weight;
    }
}

public sealed record CapNominationMetricAdjuster(double Cap) : INominationMetricFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(NominationMetric item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

/// <summary>Runs the nomination metric steps in order and keeps a trace of each.</summary>
public sealed class NominationMetricEngine(List<INominationMetricFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<INominationMetricFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static NominationMetricEngine DefaultNominationMetricEngine(Registry flags)
    {
        return new NominationMetricEngine(
            new List<INominationMetricFactor>
            {
                new FlaggedNominationMetricAdjuster(flags, "nomination-metric-default", 1.6),
                new PercentNominationMetricAdjuster(3.6),
                new NameNominationMetricAdjuster(
                    new Dictionary<string, double> { ["online"] = 0.77, ["retail"] = 0.88, ["west"] = 0.81 }),
                new TieredNominationMetricAdjuster(
                    new List<double> { 4993.0, 5565.0, 7836.0, 9148.0 },
                    new List<double> { 1.14, 0.9, 1.05, 0.94 }),
                new NominationMetricCostOffsetAdjuster(0.08),
                new CapNominationMetricAdjuster(48271.0),
            },
            new List<string>(),
            4.0,
            26048.6);
    }

    public double Evaluate(NominationMetric item)
    {
        var current = item.EffectiveLoad();
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

    public Dictionary<string, double> EvaluateAll(List<NominationMetric> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
