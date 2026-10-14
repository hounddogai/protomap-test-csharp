namespace Acme.Workforce.Compensation.PayBand.Milestone;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the pay band bundle calculation.</summary>
public interface IPayBandBundleModifier
{
    string Describe();
    double Adjust(PayBandBundle item, double valueValue);
}

public sealed record CapPayBandBundleAdjuster(double Cap) : IPayBandBundleModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(PayBandBundle item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record FlaggedPayBandBundleAdjuster(Registry Flags, string Flag, double Percent) : IPayBandBundleModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(PayBandBundle item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record NamePayBandBundleAdjuster(Dictionary<string, double> Factors) : IPayBandBundleModifier
{
    public string Describe()
    {
        return "by name";
    }

    public double Adjust(PayBandBundle item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record TieredPayBandBundleAdjuster(List<double> Bounds, List<double> Rates) : IPayBandBundleModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(PayBandBundle item, double valueValue)
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

public sealed record PercentPayBandBundleAdjuster(double Percent) : IPayBandBundleModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(PayBandBundle item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the pay band bundle steps in order and keeps a trace of each.</summary>
public sealed class PayBandBundleEngine(List<IPayBandBundleModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IPayBandBundleModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static PayBandBundleEngine DefaultPayBandBundleEngine(Registry flags)
    {
        return new PayBandBundleEngine(
            new List<IPayBandBundleModifier>
            {
                new CapPayBandBundleAdjuster(48009.0),
                new FlaggedPayBandBundleAdjuster(flags, "pay-band-bundle-seasonal", 9.4),
                new NamePayBandBundleAdjuster(
                    new Dictionary<string, double> { ["west"] = 0.91, ["online"] = 1.18, ["central"] = 1.3 }),
                new TieredPayBandBundleAdjuster(
                    new List<double> { 2193.0, 3707.0, 7283.0, 9107.0 },
                    new List<double> { 1.2, 1.06, 1.15, 0.89 }),
                new PercentPayBandBundleAdjuster(-2.4),
            },
            new List<string>(),
            4.3,
            33390.1);
    }

    public double Price(PayBandBundle item)
    {
        var current = item.EffectiveVolume();
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

    public Dictionary<string, double> EvaluateAll(List<PayBandBundle> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
