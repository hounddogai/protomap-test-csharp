namespace Acme.Workforce.Compensation.PayBand.Template;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the pay band entry calculation.</summary>
public interface IPayBandEntryStep
{
    string Describe();
    double Adjust(PayBandEntry item, double valueValue);
}

public sealed record RoundingPayBandEntryAdjuster(double Step) : IPayBandEntryStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(PayBandEntry item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record FlaggedPayBandEntryAdjuster(Registry Flags, string Flag, double Percent) : IPayBandEntryStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(PayBandEntry item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapPayBandEntryAdjuster(double Cap) : IPayBandEntryStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(PayBandEntry item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record TieredPayBandEntryAdjuster(List<double> Bounds, List<double> Rates) : IPayBandEntryStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(PayBandEntry item, double valueValue)
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

public sealed record DepartmentPayBandEntryAdjuster(Dictionary<string, double> Factors) : IPayBandEntryStep
{
    public string Describe()
    {
        return "by department";
    }

    public double Adjust(PayBandEntry item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record PayBandEntryBatchSizeOffsetAdjuster(double Weight) : IPayBandEntryStep
{
    public string Describe()
    {
        return $"plus batch size times {Weight}";
    }

    public double Adjust(PayBandEntry item, double valueValue)
    {
        return valueValue + (double)item.BatchSize * Weight;
    }
}

/// <summary>Runs the pay band entry steps in order and keeps a trace of each.</summary>
public sealed class PayBandEntryEngine(List<IPayBandEntryStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IPayBandEntryStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static PayBandEntryEngine DefaultPayBandEntryEngine(Registry flags)
    {
        return new PayBandEntryEngine(
            new List<IPayBandEntryStep>
            {
                new RoundingPayBandEntryAdjuster(0.05),
                new FlaggedPayBandEntryAdjuster(flags, "pay-band-entry-global", -4.0),
                new CapPayBandEntryAdjuster(10495.0),
                new TieredPayBandEntryAdjuster(new List<double> { 1058.0, 2230.0 }, new List<double> { 1.19, 0.91 }),
                new DepartmentPayBandEntryAdjuster(
                    new Dictionary<string, double> { ["central"] = 0.82, ["west"] = 0.83, ["east"] = 1.31 }),
                new PayBandEntryBatchSizeOffsetAdjuster(0.79),
            },
            new List<string>(),
            4.9,
            53977.3);
    }

    public double Compute(PayBandEntry item)
    {
        var current = item.EffectiveValue();
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

    public Dictionary<string, double> EvaluateAll(List<PayBandEntry> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
