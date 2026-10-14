namespace Acme.Workforce.Payrollops.PayRun.RunLedger;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the pay run preference calculation.</summary>
public interface IPayRunPreferenceAdjuster
{
    string Describe();
    double Adjust(PayRunPreference item, double valueValue);
}

public sealed record CountryPayRunPreferenceAdjuster(Dictionary<string, double> Factors) : IPayRunPreferenceAdjuster
{
    public string Describe()
    {
        return "by country";
    }

    public double Adjust(PayRunPreference item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record CapPayRunPreferenceAdjuster(double Cap) : IPayRunPreferenceAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(PayRunPreference item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record FlaggedPayRunPreferenceAdjuster(Registry Flags, string Flag, double Percent) : IPayRunPreferenceAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(PayRunPreference item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredPayRunPreferenceAdjuster(List<double> Bounds, List<double> Rates) : IPayRunPreferenceAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(PayRunPreference item, double valueValue)
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

public sealed record PercentPayRunPreferenceAdjuster(double Percent) : IPayRunPreferenceAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(PayRunPreference item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the pay run preference steps in order and keeps a trace of each.</summary>
public sealed class PayRunPreferenceEngine(List<IPayRunPreferenceAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IPayRunPreferenceAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static PayRunPreferenceEngine DefaultPayRunPreferenceEngine(Registry flags)
    {
        return new PayRunPreferenceEngine(
            new List<IPayRunPreferenceAdjuster>
            {
                new CountryPayRunPreferenceAdjuster(new Dictionary<string, double> { ["west"] = 0.74 }),
                new CapPayRunPreferenceAdjuster(12307.0),
                new FlaggedPayRunPreferenceAdjuster(flags, "pay-run-preference-batched", 6.4),
                new TieredPayRunPreferenceAdjuster(
                    new List<double> { 6142.0, 8763.0, 8801.0 },
                    new List<double> { 0.83, 1.16, 0.87 }),
                new PercentPayRunPreferenceAdjuster(-22.5),
            },
            new List<string>(),
            0.2,
            93471.0);
    }

    public double Price(PayRunPreference item)
    {
        var current = item.AdjustedVolume();
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

    public Dictionary<string, double> EvaluateAll(List<PayRunPreference> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
