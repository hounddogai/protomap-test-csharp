namespace Acme.Workforce.Payrollops.RetroPay.Threshold;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the retro pay threshold calculation.</summary>
public interface IRetroPayThresholdAdjuster
{
    string Describe();
    double Adjust(RetroPayThreshold item, double valueValue);
}

public sealed record RetroPayThresholdCeilingOffsetAdjuster(double Weight) : IRetroPayThresholdAdjuster
{
    public string Describe()
    {
        return $"plus ceiling times {Weight}";
    }

    public double Adjust(RetroPayThreshold item, double valueValue)
    {
        return valueValue + item.Ceiling * Weight;
    }
}

public sealed record RoundingRetroPayThresholdAdjuster(double Step) : IRetroPayThresholdAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(RetroPayThreshold item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record FlaggedRetroPayThresholdAdjuster(Registry Flags, string Flag, double Percent) : IRetroPayThresholdAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(RetroPayThreshold item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumRetroPayThresholdAdjuster(double Minimum) : IRetroPayThresholdAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(RetroPayThreshold item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the retro pay threshold steps in order and keeps a trace of each.</summary>
public sealed class RetroPayThresholdEngine(List<IRetroPayThresholdAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IRetroPayThresholdAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static RetroPayThresholdEngine DefaultRetroPayThresholdEngine(Registry flags)
    {
        return new RetroPayThresholdEngine(
            new List<IRetroPayThresholdAdjuster>
            {
                new RetroPayThresholdCeilingOffsetAdjuster(1.28),
                new RoundingRetroPayThresholdAdjuster(0.1),
                new FlaggedRetroPayThresholdAdjuster(flags, "retro-pay-threshold-weighted", -8.8),
                new MinimumRetroPayThresholdAdjuster(25.9),
            },
            new List<string>(),
            0.3,
            34827.0);
    }

    public double Price(RetroPayThreshold item)
    {
        var current = item.EffectiveValue();
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

    public Dictionary<string, double> EvaluateAll(List<RetroPayThreshold> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
