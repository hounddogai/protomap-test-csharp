namespace Acme.Workforce.Assets.LicenseSeat.RecordItem;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the fallback license seat record calculation.</summary>
public interface IFallbackLicenseSeatRecordFactor
{
    string Describe();
    double Adjust(FallbackLicenseSeatRecord item, double valueValue);
}

public sealed record PercentFallbackLicenseSeatRecordAdjuster(double Percent) : IFallbackLicenseSeatRecordFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(FallbackLicenseSeatRecord item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record FallbackLicenseSeatRecordVersionOffsetAdjuster(double Weight) : IFallbackLicenseSeatRecordFactor
{
    public string Describe()
    {
        return $"plus version times {Weight}";
    }

    public double Adjust(FallbackLicenseSeatRecord item, double valueValue)
    {
        return valueValue + (double)item.Version * Weight;
    }
}

public sealed record SourceFallbackLicenseSeatRecordAdjuster(Dictionary<string, double> Factors) : IFallbackLicenseSeatRecordFactor
{
    public string Describe()
    {
        return "by source";
    }

    public double Adjust(FallbackLicenseSeatRecord item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record RoundingFallbackLicenseSeatRecordAdjuster(double Step) : IFallbackLicenseSeatRecordFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(FallbackLicenseSeatRecord item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record MinimumFallbackLicenseSeatRecordAdjuster(double Minimum) : IFallbackLicenseSeatRecordFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(FallbackLicenseSeatRecord item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record FlaggedFallbackLicenseSeatRecordAdjuster(Registry Flags, string Flag, double Percent) : IFallbackLicenseSeatRecordFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(FallbackLicenseSeatRecord item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the fallback license seat record steps in order and keeps a trace of each.</summary>
public sealed class FallbackLicenseSeatRecordEngine(List<IFallbackLicenseSeatRecordFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IFallbackLicenseSeatRecordFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static FallbackLicenseSeatRecordEngine DefaultFallbackLicenseSeatRecordEngine(Registry flags)
    {
        return new FallbackLicenseSeatRecordEngine(
            new List<IFallbackLicenseSeatRecordFactor>
            {
                new PercentFallbackLicenseSeatRecordAdjuster(-9.4),
                new FallbackLicenseSeatRecordVersionOffsetAdjuster(1.75),
                new SourceFallbackLicenseSeatRecordAdjuster(
                    new Dictionary<string, double> { ["south"] = 0.87, ["west"] = 1.25, ["east"] = 0.98 }),
                new RoundingFallbackLicenseSeatRecordAdjuster(1.0),
                new MinimumFallbackLicenseSeatRecordAdjuster(35.9),
                new FlaggedFallbackLicenseSeatRecordAdjuster(flags, "fallback-license-seat-record-seasonal", 4.3),
            },
            new List<string>(),
            4.3,
            11313.3);
    }

    public double Evaluate(FallbackLicenseSeatRecord item)
    {
        var current = item.AdjustedTotal();
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

    public Dictionary<string, double> EvaluateAll(List<FallbackLicenseSeatRecord> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
