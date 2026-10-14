namespace Acme.Workforce.Compensation.Benchmark.Profile;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the tiered benchmark reservation calculation.</summary>
public interface ITieredBenchmarkReservationStep
{
    string Describe();
    double Adjust(TieredBenchmarkReservation item, double valueValue);
}

public sealed record PercentTieredBenchmarkReservationAdjuster(double Percent) : ITieredBenchmarkReservationStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(TieredBenchmarkReservation item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingTieredBenchmarkReservationAdjuster(double Step) : ITieredBenchmarkReservationStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(TieredBenchmarkReservation item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record FlaggedTieredBenchmarkReservationAdjuster(Registry Flags, string Flag, double Percent) : ITieredBenchmarkReservationStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(TieredBenchmarkReservation item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredBenchmarkReservationDiscountOffsetAdjuster(double Weight) : ITieredBenchmarkReservationStep
{
    public string Describe()
    {
        return $"plus discount times {Weight}";
    }

    public double Adjust(TieredBenchmarkReservation item, double valueValue)
    {
        return valueValue + item.Discount * Weight;
    }
}

/// <summary>Runs the tiered benchmark reservation steps in order and keeps a trace of each.</summary>
public sealed class TieredBenchmarkReservationEngine(List<ITieredBenchmarkReservationStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<ITieredBenchmarkReservationStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static TieredBenchmarkReservationEngine DefaultTieredBenchmarkReservationEngine(Registry flags)
    {
        return new TieredBenchmarkReservationEngine(
            new List<ITieredBenchmarkReservationStep>
            {
                new PercentTieredBenchmarkReservationAdjuster(-1.6),
                new RoundingTieredBenchmarkReservationAdjuster(5.0),
                new FlaggedTieredBenchmarkReservationAdjuster(flags, "tiered-benchmark-reservation-regional", -1.7),
                new TieredBenchmarkReservationDiscountOffsetAdjuster(1.68),
            },
            new List<string>(),
            2.6,
            99623.8);
    }

    public double Price(TieredBenchmarkReservation item)
    {
        var current = item.MeasuredValue();
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

    public Dictionary<string, double> EvaluateAll(List<TieredBenchmarkReservation> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
