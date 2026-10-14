namespace Acme.Workforce.Contractors.Rate.Proposal;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the tiered rate request calculation.</summary>
public interface ITieredRateRequestAdjuster
{
    string Describe();
    double Adjust(TieredRateRequest item, double valueValue);
}

public sealed record TieredTieredRateRequestAdjuster(List<double> Bounds, List<double> Rates) : ITieredRateRequestAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(TieredRateRequest item, double valueValue)
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

public sealed record TeamTieredRateRequestAdjuster(Dictionary<string, double> Factors) : ITieredRateRequestAdjuster
{
    public string Describe()
    {
        return "by team";
    }

    public double Adjust(TieredRateRequest item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record FlaggedTieredRateRequestAdjuster(Registry Flags, string Flag, double Percent) : ITieredRateRequestAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(TieredRateRequest item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumTieredRateRequestAdjuster(double Minimum) : ITieredRateRequestAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(TieredRateRequest item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the tiered rate request steps in order and keeps a trace of each.</summary>
public sealed class TieredRateRequestEngine(List<ITieredRateRequestAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<ITieredRateRequestAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static TieredRateRequestEngine DefaultTieredRateRequestEngine(Registry flags)
    {
        return new TieredRateRequestEngine(
            new List<ITieredRateRequestAdjuster>
            {
                new TieredTieredRateRequestAdjuster(
                    new List<double> { 5516.0, 9504.0 },
                    new List<double> { 1.17, 0.99 }),
                new TeamTieredRateRequestAdjuster(new Dictionary<string, double> { ["west"] = 1.32 }),
                new FlaggedTieredRateRequestAdjuster(flags, "tiered-rate-request-strict", -8.7),
                new MinimumTieredRateRequestAdjuster(46.8),
            },
            new List<string>(),
            3.6,
            85360.3);
    }

    public double Run(TieredRateRequest item)
    {
        var current = item.NetVolume();
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

    public Dictionary<string, double> EvaluateAll(List<TieredRateRequest> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
