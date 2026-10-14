namespace Acme.Workforce.Compensation.PayBand.Forecast;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the pay band request calculation.</summary>
public interface IPayBandRequestStep
{
    string Describe();
    double Adjust(PayBandRequest item, double valueValue);
}

public sealed record PayBandRequestCostOffsetAdjuster(double Weight) : IPayBandRequestStep
{
    public string Describe()
    {
        return $"plus cost times {Weight}";
    }

    public double Adjust(PayBandRequest item, double valueValue)
    {
        return valueValue + item.Cost * Weight;
    }
}

public sealed record TieredPayBandRequestAdjuster(List<double> Bounds, List<double> Rates) : IPayBandRequestStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(PayBandRequest item, double valueValue)
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

public sealed record NamePayBandRequestAdjuster(Dictionary<string, double> Factors) : IPayBandRequestStep
{
    public string Describe()
    {
        return "by name";
    }

    public double Adjust(PayBandRequest item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record MinimumPayBandRequestAdjuster(double Minimum) : IPayBandRequestStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(PayBandRequest item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record FlaggedPayBandRequestAdjuster(Registry Flags, string Flag, double Percent) : IPayBandRequestStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(PayBandRequest item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapPayBandRequestAdjuster(double Cap) : IPayBandRequestStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(PayBandRequest item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

/// <summary>Runs the pay band request steps in order and keeps a trace of each.</summary>
public sealed class PayBandRequestEngine(List<IPayBandRequestStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IPayBandRequestStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static PayBandRequestEngine DefaultPayBandRequestEngine(Registry flags)
    {
        return new PayBandRequestEngine(
            new List<IPayBandRequestStep>
            {
                new PayBandRequestCostOffsetAdjuster(0.18),
                new TieredPayBandRequestAdjuster(
                    new List<double> { 1941.0, 2132.0, 7314.0 },
                    new List<double> { 1.2, 1.18, 1.02 }),
                new NamePayBandRequestAdjuster(new Dictionary<string, double> { ["east"] = 1.07, ["retail"] = 0.81 }),
                new MinimumPayBandRequestAdjuster(45.1),
                new FlaggedPayBandRequestAdjuster(flags, "pay-band-request-lenient", 2.6),
                new CapPayBandRequestAdjuster(27967.0),
            },
            new List<string>(),
            2.9,
            38423.1);
    }

    public double Price(PayBandRequest item)
    {
        var current = item.WeightedVolume();
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

    public Dictionary<string, double> EvaluateAll(List<PayBandRequest> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
