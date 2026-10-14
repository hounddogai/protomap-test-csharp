namespace Acme.Workforce.Compensation.PayBand.Cycle;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the pay band cycle calculation.</summary>
public interface IPayBandCycleAdjuster
{
    string Describe();
    double Adjust(PayBandCycle item, double valueValue);
}

public sealed record CapPayBandCycleAdjuster(double Cap) : IPayBandCycleAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(PayBandCycle item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record PercentPayBandCycleAdjuster(double Percent) : IPayBandCycleAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(PayBandCycle item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record FlaggedPayBandCycleAdjuster(Registry Flags, string Flag, double Percent) : IPayBandCycleAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(PayBandCycle item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the pay band cycle steps in order and keeps a trace of each.</summary>
public sealed class PayBandCycleEngine(List<IPayBandCycleAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IPayBandCycleAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static PayBandCycleEngine DefaultPayBandCycleEngine(Registry flags)
    {
        return new PayBandCycleEngine(
            new List<IPayBandCycleAdjuster>
            {
                new CapPayBandCycleAdjuster(37919.0),
                new PercentPayBandCycleAdjuster(-4.9),
                new FlaggedPayBandCycleAdjuster(flags, "pay-band-cycle-primary", 9.5),
            },
            new List<string>(),
            5.0,
            59462.1);
    }

    public double Run(PayBandCycle item)
    {
        var current = item.NetVolume();
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

    public Dictionary<string, double> EvaluateAll(List<PayBandCycle> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
