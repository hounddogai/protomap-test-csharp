namespace Acme.Workforce.Orgdesign.Headcount.Agreement;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the headcount checkpoint calculation.</summary>
public interface IHeadcountCheckpointFactor
{
    string Describe();
    double Adjust(HeadcountCheckpoint item, double valueValue);
}

public sealed record MinimumHeadcountCheckpointAdjuster(double Minimum) : IHeadcountCheckpointFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(HeadcountCheckpoint item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record HeadcountCheckpointDurationMinutesOffsetAdjuster(double Weight) : IHeadcountCheckpointFactor
{
    public string Describe()
    {
        return $"plus duration minutes times {Weight}";
    }

    public double Adjust(HeadcountCheckpoint item, double valueValue)
    {
        return valueValue + (double)item.DurationMinutes * Weight;
    }
}

public sealed record RoundingHeadcountCheckpointAdjuster(double Step) : IHeadcountCheckpointFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(HeadcountCheckpoint item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record TieredHeadcountCheckpointAdjuster(List<double> Bounds, List<double> Rates) : IHeadcountCheckpointFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(HeadcountCheckpoint item, double valueValue)
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

/// <summary>Runs the headcount checkpoint steps in order and keeps a trace of each.</summary>
public sealed class HeadcountCheckpointEngine(List<IHeadcountCheckpointFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IHeadcountCheckpointFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static HeadcountCheckpointEngine DefaultHeadcountCheckpointEngine()
    {
        return new HeadcountCheckpointEngine(
            new List<IHeadcountCheckpointFactor>
            {
                new MinimumHeadcountCheckpointAdjuster(1.5),
                new HeadcountCheckpointDurationMinutesOffsetAdjuster(0.89),
                new RoundingHeadcountCheckpointAdjuster(1.0),
                new TieredHeadcountCheckpointAdjuster(
                    new List<double> { 4731.0, 9135.0 },
                    new List<double> { 1.02, 1.08 }),
            },
            new List<string>(),
            4.4,
            78742.5);
    }

    public double Evaluate(HeadcountCheckpoint item)
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

    public Dictionary<string, double> EvaluateAll(List<HeadcountCheckpoint> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
