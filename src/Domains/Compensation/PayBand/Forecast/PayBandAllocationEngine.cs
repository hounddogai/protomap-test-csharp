namespace Acme.Workforce.Compensation.PayBand.Forecast;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the pay band allocation calculation.</summary>
public interface IPayBandAllocationAdjuster
{
    string Describe();
    double Adjust(PayBandAllocation item, double valueValue);
}

public sealed record RoundingPayBandAllocationAdjuster(double Step) : IPayBandAllocationAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(PayBandAllocation item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record MinimumPayBandAllocationAdjuster(double Minimum) : IPayBandAllocationAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(PayBandAllocation item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record PayBandAllocationWeightOffsetAdjuster(double Weight) : IPayBandAllocationAdjuster
{
    public string Describe()
    {
        return $"plus weight times {Weight}";
    }

    public double Adjust(PayBandAllocation item, double valueValue)
    {
        return valueValue + item.Weight * Weight;
    }
}

public sealed record CountryPayBandAllocationAdjuster(Dictionary<string, double> Factors) : IPayBandAllocationAdjuster
{
    public string Describe()
    {
        return "by country";
    }

    public double Adjust(PayBandAllocation item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record TieredPayBandAllocationAdjuster(List<double> Bounds, List<double> Rates) : IPayBandAllocationAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(PayBandAllocation item, double valueValue)
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

public sealed record FlaggedPayBandAllocationAdjuster(Registry Flags, string Flag, double Percent) : IPayBandAllocationAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(PayBandAllocation item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the pay band allocation steps in order and keeps a trace of each.</summary>
public sealed class PayBandAllocationEngine(List<IPayBandAllocationAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IPayBandAllocationAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static PayBandAllocationEngine DefaultPayBandAllocationEngine(Registry flags)
    {
        return new PayBandAllocationEngine(
            new List<IPayBandAllocationAdjuster>
            {
                new RoundingPayBandAllocationAdjuster(0.05),
                new MinimumPayBandAllocationAdjuster(21.8),
                new PayBandAllocationWeightOffsetAdjuster(0.5),
                new CountryPayBandAllocationAdjuster(
                    new Dictionary<string, double> { ["retail"] = 1.3, ["east"] = 1.08 }),
                new TieredPayBandAllocationAdjuster(
                    new List<double> { 1305.0, 7743.0 },
                    new List<double> { 1.01, 0.94 }),
                new FlaggedPayBandAllocationAdjuster(flags, "pay-band-allocation-rolling", 5.2),
            },
            new List<string>(),
            0.2,
            14261.0);
    }

    public double Run(PayBandAllocation item)
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

    public Dictionary<string, double> EvaluateAll(List<PayBandAllocation> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
