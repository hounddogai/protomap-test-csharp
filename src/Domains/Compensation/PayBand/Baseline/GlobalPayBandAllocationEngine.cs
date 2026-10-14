namespace Acme.Workforce.Compensation.PayBand.Baseline;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the global pay band allocation calculation.</summary>
public interface IGlobalPayBandAllocationModifier
{
    string Describe();
    double Adjust(GlobalPayBandAllocation item, double valueValue);
}

public sealed record CapGlobalPayBandAllocationAdjuster(double Cap) : IGlobalPayBandAllocationModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(GlobalPayBandAllocation item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record FlaggedGlobalPayBandAllocationAdjuster(Registry Flags, string Flag, double Percent) : IGlobalPayBandAllocationModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(GlobalPayBandAllocation item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingGlobalPayBandAllocationAdjuster(double Step) : IGlobalPayBandAllocationModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(GlobalPayBandAllocation item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record PercentGlobalPayBandAllocationAdjuster(double Percent) : IGlobalPayBandAllocationModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(GlobalPayBandAllocation item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumGlobalPayBandAllocationAdjuster(double Minimum) : IGlobalPayBandAllocationModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(GlobalPayBandAllocation item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record GlobalPayBandAllocationCostOffsetAdjuster(double Weight) : IGlobalPayBandAllocationModifier
{
    public string Describe()
    {
        return $"plus cost times {Weight}";
    }

    public double Adjust(GlobalPayBandAllocation item, double valueValue)
    {
        return valueValue + item.Cost * Weight;
    }
}

/// <summary>Runs the global pay band allocation steps in order and keeps a trace of each.</summary>
public sealed class GlobalPayBandAllocationEngine(List<IGlobalPayBandAllocationModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IGlobalPayBandAllocationModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static GlobalPayBandAllocationEngine DefaultGlobalPayBandAllocationEngine(Registry flags)
    {
        return new GlobalPayBandAllocationEngine(
            new List<IGlobalPayBandAllocationModifier>
            {
                new CapGlobalPayBandAllocationAdjuster(33815.0),
                new FlaggedGlobalPayBandAllocationAdjuster(flags, "global-pay-band-allocation-manual", -7.1),
                new RoundingGlobalPayBandAllocationAdjuster(0.05),
                new PercentGlobalPayBandAllocationAdjuster(-5.8),
                new MinimumGlobalPayBandAllocationAdjuster(7.4),
                new GlobalPayBandAllocationCostOffsetAdjuster(1.74),
            },
            new List<string>(),
            3.0,
            51810.3);
    }

    public double Compute(GlobalPayBandAllocation item)
    {
        var current = item.MeasuredValue();
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

    public Dictionary<string, double> EvaluateAll(List<GlobalPayBandAllocation> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
