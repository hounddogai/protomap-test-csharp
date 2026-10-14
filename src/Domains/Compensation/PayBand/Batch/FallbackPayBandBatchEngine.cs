namespace Acme.Workforce.Compensation.PayBand.Batch;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the fallback pay band batch calculation.</summary>
public interface IFallbackPayBandBatchModifier
{
    string Describe();
    double Adjust(FallbackPayBandBatch item, double valueValue);
}

public sealed record FlaggedFallbackPayBandBatchAdjuster(Registry Flags, string Flag, double Percent) : IFallbackPayBandBatchModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(FallbackPayBandBatch item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record FallbackPayBandBatchCostOffsetAdjuster(double Weight) : IFallbackPayBandBatchModifier
{
    public string Describe()
    {
        return $"plus cost times {Weight}";
    }

    public double Adjust(FallbackPayBandBatch item, double valueValue)
    {
        return valueValue + item.Cost * Weight;
    }
}

public sealed record CapFallbackPayBandBatchAdjuster(double Cap) : IFallbackPayBandBatchModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(FallbackPayBandBatch item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record RoundingFallbackPayBandBatchAdjuster(double Step) : IFallbackPayBandBatchModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(FallbackPayBandBatch item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record MinimumFallbackPayBandBatchAdjuster(double Minimum) : IFallbackPayBandBatchModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(FallbackPayBandBatch item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record TitleFallbackPayBandBatchAdjuster(Dictionary<string, double> Factors) : IFallbackPayBandBatchModifier
{
    public string Describe()
    {
        return "by title";
    }

    public double Adjust(FallbackPayBandBatch item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the fallback pay band batch steps in order and keeps a trace of each.</summary>
public sealed class FallbackPayBandBatchEngine(List<IFallbackPayBandBatchModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IFallbackPayBandBatchModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static FallbackPayBandBatchEngine DefaultFallbackPayBandBatchEngine(Registry flags)
    {
        return new FallbackPayBandBatchEngine(
            new List<IFallbackPayBandBatchModifier>
            {
                new FlaggedFallbackPayBandBatchAdjuster(flags, "fallback-pay-band-batch-tiered", 2.0),
                new FallbackPayBandBatchCostOffsetAdjuster(0.79),
                new CapFallbackPayBandBatchAdjuster(37677.0),
                new RoundingFallbackPayBandBatchAdjuster(0.1),
                new MinimumFallbackPayBandBatchAdjuster(40.2),
                new TitleFallbackPayBandBatchAdjuster(
                    new Dictionary<string, double> { ["central"] = 0.88, ["east"] = 0.89 }),
            },
            new List<string>(),
            2.8,
            50736.6);
    }

    public double Run(FallbackPayBandBatch item)
    {
        var current = item.EffectiveTotal();
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

    public Dictionary<string, double> EvaluateAll(List<FallbackPayBandBatch> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
