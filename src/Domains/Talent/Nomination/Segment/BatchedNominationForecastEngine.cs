namespace Acme.Workforce.Talent.Nomination.Segment;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the batched nomination forecast calculation.</summary>
public interface IBatchedNominationForecastAdjuster
{
    string Describe();
    double Adjust(BatchedNominationForecast item, double valueValue);
}

public sealed record MinimumBatchedNominationForecastAdjuster(double Minimum) : IBatchedNominationForecastAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(BatchedNominationForecast item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record BatchedNominationForecastCapacityOffsetAdjuster(double Weight) : IBatchedNominationForecastAdjuster
{
    public string Describe()
    {
        return $"plus capacity times {Weight}";
    }

    public double Adjust(BatchedNominationForecast item, double valueValue)
    {
        return valueValue + (double)item.Capacity * Weight;
    }
}

public sealed record RoundingBatchedNominationForecastAdjuster(double Step) : IBatchedNominationForecastAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(BatchedNominationForecast item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record CapBatchedNominationForecastAdjuster(double Cap) : IBatchedNominationForecastAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(BatchedNominationForecast item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record FlaggedBatchedNominationForecastAdjuster(Registry Flags, string Flag, double Percent) : IBatchedNominationForecastAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(BatchedNominationForecast item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the batched nomination forecast steps in order and keeps a trace of each.</summary>
public sealed class BatchedNominationForecastEngine(List<IBatchedNominationForecastAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IBatchedNominationForecastAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static BatchedNominationForecastEngine DefaultBatchedNominationForecastEngine(Registry flags)
    {
        return new BatchedNominationForecastEngine(
            new List<IBatchedNominationForecastAdjuster>
            {
                new MinimumBatchedNominationForecastAdjuster(1.6),
                new BatchedNominationForecastCapacityOffsetAdjuster(0.86),
                new RoundingBatchedNominationForecastAdjuster(0.5),
                new CapBatchedNominationForecastAdjuster(756.0),
                new FlaggedBatchedNominationForecastAdjuster(flags, "batched-nomination-forecast-capped", -6.0),
            },
            new List<string>(),
            0.0,
            12875.8);
    }

    public double Evaluate(BatchedNominationForecast item)
    {
        var current = item.AdjustedVolume();
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

    public Dictionary<string, double> EvaluateAll(List<BatchedNominationForecast> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
