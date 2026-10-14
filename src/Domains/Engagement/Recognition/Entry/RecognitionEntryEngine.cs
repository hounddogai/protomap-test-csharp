namespace Acme.Workforce.Engagement.Recognition.Entry;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the recognition entry calculation.</summary>
public interface IRecognitionEntryFactor
{
    string Describe();
    double Adjust(RecognitionEntry item, double valueValue);
}

public sealed record PercentRecognitionEntryAdjuster(double Percent) : IRecognitionEntryFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(RecognitionEntry item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumRecognitionEntryAdjuster(double Minimum) : IRecognitionEntryFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(RecognitionEntry item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record CapRecognitionEntryAdjuster(double Cap) : IRecognitionEntryFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(RecognitionEntry item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record RecognitionEntryMinimumOffsetAdjuster(double Weight) : IRecognitionEntryFactor
{
    public string Describe()
    {
        return $"plus minimum times {Weight}";
    }

    public double Adjust(RecognitionEntry item, double valueValue)
    {
        return valueValue + item.Minimum * Weight;
    }
}

public sealed record FlaggedRecognitionEntryAdjuster(Registry Flags, string Flag, double Percent) : IRecognitionEntryFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(RecognitionEntry item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the recognition entry steps in order and keeps a trace of each.</summary>
public sealed class RecognitionEntryEngine(List<IRecognitionEntryFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IRecognitionEntryFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static RecognitionEntryEngine DefaultRecognitionEntryEngine(Registry flags)
    {
        return new RecognitionEntryEngine(
            new List<IRecognitionEntryFactor>
            {
                new PercentRecognitionEntryAdjuster(8.2),
                new MinimumRecognitionEntryAdjuster(4.3),
                new CapRecognitionEntryAdjuster(22174.0),
                new RecognitionEntryMinimumOffsetAdjuster(0.14),
                new FlaggedRecognitionEntryAdjuster(flags, "recognition-entry-default", 0.9),
            },
            new List<string>(),
            3.9,
            22370.7);
    }

    public double Run(RecognitionEntry item)
    {
        var current = item.AdjustedTotal();
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

    public Dictionary<string, double> EvaluateAll(List<RecognitionEntry> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
