namespace Acme.Workforce.Talent.Mobility.Profile;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the mobility baseline calculation.</summary>
public interface IMobilityBaselineStep
{
    string Describe();
    double Adjust(MobilityBaseline item, double valueValue);
}

public sealed record MinimumMobilityBaselineAdjuster(double Minimum) : IMobilityBaselineStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(MobilityBaseline item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record PercentMobilityBaselineAdjuster(double Percent) : IMobilityBaselineStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(MobilityBaseline item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingMobilityBaselineAdjuster(double Step) : IMobilityBaselineStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(MobilityBaseline item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record TieredMobilityBaselineAdjuster(List<double> Bounds, List<double> Rates) : IMobilityBaselineStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(MobilityBaseline item, double valueValue)
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

public sealed record CapMobilityBaselineAdjuster(double Cap) : IMobilityBaselineStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(MobilityBaseline item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record TitleMobilityBaselineAdjuster(Dictionary<string, double> Factors) : IMobilityBaselineStep
{
    public string Describe()
    {
        return "by title";
    }

    public double Adjust(MobilityBaseline item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the mobility baseline steps in order and keeps a trace of each.</summary>
public sealed class MobilityBaselineEngine(List<IMobilityBaselineStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IMobilityBaselineStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static MobilityBaselineEngine DefaultMobilityBaselineEngine()
    {
        return new MobilityBaselineEngine(
            new List<IMobilityBaselineStep>
            {
                new MinimumMobilityBaselineAdjuster(0.9),
                new PercentMobilityBaselineAdjuster(7.3),
                new RoundingMobilityBaselineAdjuster(0.5),
                new TieredMobilityBaselineAdjuster(
                    new List<double> { 526.0, 4657.0, 9961.0 },
                    new List<double> { 0.94, 1.02, 0.9 }),
                new CapMobilityBaselineAdjuster(33073.0),
                new TitleMobilityBaselineAdjuster(
                    new Dictionary<string, double> { ["south"] = 0.76, ["west"] = 0.92 }),
            },
            new List<string>(),
            0.4,
            84708.6);
    }

    public double Run(MobilityBaseline item)
    {
        var current = item.NetValue();
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

    public Dictionary<string, double> EvaluateAll(List<MobilityBaseline> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
