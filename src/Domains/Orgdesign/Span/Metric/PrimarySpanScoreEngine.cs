namespace Acme.Workforce.Orgdesign.Span.Metric;

/// <summary>One step of the primary span score calculation.</summary>
public interface IPrimarySpanScoreAdjuster
{
    string Describe();
    double Adjust(PrimarySpanScore item, double valueValue);
}

public sealed record CapPrimarySpanScoreAdjuster(double Cap) : IPrimarySpanScoreAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(PrimarySpanScore item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record MinimumPrimarySpanScoreAdjuster(double Minimum) : IPrimarySpanScoreAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(PrimarySpanScore item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record PercentPrimarySpanScoreAdjuster(double Percent) : IPrimarySpanScoreAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(PrimarySpanScore item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the primary span score steps in order and keeps a trace of each.</summary>
public sealed class PrimarySpanScoreEngine(List<IPrimarySpanScoreAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IPrimarySpanScoreAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static PrimarySpanScoreEngine DefaultPrimarySpanScoreEngine()
    {
        return new PrimarySpanScoreEngine(
            new List<IPrimarySpanScoreAdjuster>
            {
                new CapPrimarySpanScoreAdjuster(34219.0),
                new MinimumPrimarySpanScoreAdjuster(38.9),
                new PercentPrimarySpanScoreAdjuster(16.6),
            },
            new List<string>(),
            2.4,
            59048.5);
    }

    public double Compute(PrimarySpanScore item)
    {
        var current = item.EffectiveVolume();
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

    public Dictionary<string, double> EvaluateAll(List<PrimarySpanScore> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
