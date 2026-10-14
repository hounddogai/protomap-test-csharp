namespace Acme.Workforce.Payrollops.RetroPay.Threshold;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the retro pay rollup calculation.</summary>
public interface IRetroPayRollupAdjuster
{
    string Describe();
    double Adjust(RetroPayRollup item, double valueValue);
}

public sealed record RoundingRetroPayRollupAdjuster(double Step) : IRetroPayRollupAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(RetroPayRollup item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record TieredRetroPayRollupAdjuster(List<double> Bounds, List<double> Rates) : IRetroPayRollupAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(RetroPayRollup item, double valueValue)
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

public sealed record MinimumRetroPayRollupAdjuster(double Minimum) : IRetroPayRollupAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(RetroPayRollup item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record PercentRetroPayRollupAdjuster(double Percent) : IRetroPayRollupAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(RetroPayRollup item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record FlaggedRetroPayRollupAdjuster(Registry Flags, string Flag, double Percent) : IRetroPayRollupAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(RetroPayRollup item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the retro pay rollup steps in order and keeps a trace of each.</summary>
public sealed class RetroPayRollupEngine(List<IRetroPayRollupAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IRetroPayRollupAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static RetroPayRollupEngine DefaultRetroPayRollupEngine(Registry flags)
    {
        return new RetroPayRollupEngine(
            new List<IRetroPayRollupAdjuster>
            {
                new RoundingRetroPayRollupAdjuster(0.5),
                new TieredRetroPayRollupAdjuster(
                    new List<double> { 214.0, 5193.0, 8368.0, 8500.0 },
                    new List<double> { 0.82, 1.03, 0.92, 1.17 }),
                new MinimumRetroPayRollupAdjuster(27.5),
                new PercentRetroPayRollupAdjuster(-0.3),
                new FlaggedRetroPayRollupAdjuster(flags, "retro-pay-rollup-tiered", 3.2),
            },
            new List<string>(),
            1.4,
            25995.5);
    }

    public double Evaluate(RetroPayRollup item)
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

    public Dictionary<string, double> EvaluateAll(List<RetroPayRollup> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
