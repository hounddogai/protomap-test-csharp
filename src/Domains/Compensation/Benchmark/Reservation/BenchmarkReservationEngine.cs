namespace Acme.Workforce.Compensation.Benchmark.Reservation;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the benchmark reservation calculation.</summary>
public interface IBenchmarkReservationAdjuster
{
    string Describe();
    double Adjust(BenchmarkReservation item, double valueValue);
}

public sealed record BenchmarkReservationWindowDaysOffsetAdjuster(double Weight) : IBenchmarkReservationAdjuster
{
    public string Describe()
    {
        return $"plus window days times {Weight}";
    }

    public double Adjust(BenchmarkReservation item, double valueValue)
    {
        return valueValue + (double)item.WindowDays * Weight;
    }
}

public sealed record TieredBenchmarkReservationAdjuster(List<double> Bounds, List<double> Rates) : IBenchmarkReservationAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(BenchmarkReservation item, double valueValue)
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

public sealed record CapBenchmarkReservationAdjuster(double Cap) : IBenchmarkReservationAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(BenchmarkReservation item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record TitleBenchmarkReservationAdjuster(Dictionary<string, double> Factors) : IBenchmarkReservationAdjuster
{
    public string Describe()
    {
        return "by title";
    }

    public double Adjust(BenchmarkReservation item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record RoundingBenchmarkReservationAdjuster(double Step) : IBenchmarkReservationAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(BenchmarkReservation item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the benchmark reservation steps in order and keeps a trace of each.</summary>
public sealed class BenchmarkReservationEngine(List<IBenchmarkReservationAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IBenchmarkReservationAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static BenchmarkReservationEngine DefaultBenchmarkReservationEngine()
    {
        return new BenchmarkReservationEngine(
            new List<IBenchmarkReservationAdjuster>
            {
                new BenchmarkReservationWindowDaysOffsetAdjuster(0.79),
                new TieredBenchmarkReservationAdjuster(
                    new List<double> { 3438.0, 4540.0, 5805.0, 9525.0 },
                    new List<double> { 1.08, 1.12, 1.15, 0.92 }),
                new CapBenchmarkReservationAdjuster(25379.0),
                new TitleBenchmarkReservationAdjuster(
                    new Dictionary<string, double> { ["south"] = 1.1, ["east"] = 0.99 }),
                new RoundingBenchmarkReservationAdjuster(0.25),
            },
            new List<string>(),
            0.7,
            46928.5);
    }

    public double Run(BenchmarkReservation item)
    {
        var current = item.WeightedTotal();
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

    public Dictionary<string, double> EvaluateAll(List<BenchmarkReservation> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
