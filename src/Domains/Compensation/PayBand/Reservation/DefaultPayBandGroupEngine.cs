namespace Acme.Workforce.Compensation.PayBand.Reservation;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the default pay band group calculation.</summary>
public interface IDefaultPayBandGroupStep
{
    string Describe();
    double Adjust(DefaultPayBandGroup item, double valueValue);
}

public sealed record TieredDefaultPayBandGroupAdjuster(List<double> Bounds, List<double> Rates) : IDefaultPayBandGroupStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(DefaultPayBandGroup item, double valueValue)
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

public sealed record FlaggedDefaultPayBandGroupAdjuster(Registry Flags, string Flag, double Percent) : IDefaultPayBandGroupStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(DefaultPayBandGroup item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TitleDefaultPayBandGroupAdjuster(Dictionary<string, double> Factors) : IDefaultPayBandGroupStep
{
    public string Describe()
    {
        return "by title";
    }

    public double Adjust(DefaultPayBandGroup item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record PercentDefaultPayBandGroupAdjuster(double Percent) : IDefaultPayBandGroupStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(DefaultPayBandGroup item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the default pay band group steps in order and keeps a trace of each.</summary>
public sealed class DefaultPayBandGroupEngine(List<IDefaultPayBandGroupStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IDefaultPayBandGroupStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static DefaultPayBandGroupEngine DefaultDefaultPayBandGroupEngine(Registry flags)
    {
        return new DefaultPayBandGroupEngine(
            new List<IDefaultPayBandGroupStep>
            {
                new TieredDefaultPayBandGroupAdjuster(
                    new List<double> { 5035.0, 9133.0, 9684.0 },
                    new List<double> { 0.99, 0.83, 1.05 }),
                new FlaggedDefaultPayBandGroupAdjuster(flags, "default-pay-band-group-tiered", 4.4),
                new TitleDefaultPayBandGroupAdjuster(
                    new Dictionary<string, double> { ["central"] = 1.14, ["retail"] = 0.88, ["south"] = 1.3 }),
                new PercentDefaultPayBandGroupAdjuster(6.4),
            },
            new List<string>(),
            0.9,
            44217.8);
    }

    public double Price(DefaultPayBandGroup item)
    {
        var current = item.AdjustedLoad();
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

    public Dictionary<string, double> EvaluateAll(List<DefaultPayBandGroup> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
