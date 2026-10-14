namespace Acme.Workforce.Engagement.Recognition.Snapshot;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the lenient recognition entry calculation.</summary>
public interface ILenientRecognitionEntryModifier
{
    string Describe();
    double Adjust(LenientRecognitionEntry item, double valueValue);
}

public sealed record RoundingLenientRecognitionEntryAdjuster(double Step) : ILenientRecognitionEntryModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(LenientRecognitionEntry item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record FlaggedLenientRecognitionEntryAdjuster(Registry Flags, string Flag, double Percent) : ILenientRecognitionEntryModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(LenientRecognitionEntry item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record PercentLenientRecognitionEntryAdjuster(double Percent) : ILenientRecognitionEntryModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(LenientRecognitionEntry item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapLenientRecognitionEntryAdjuster(double Cap) : ILenientRecognitionEntryModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(LenientRecognitionEntry item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record TieredLenientRecognitionEntryAdjuster(List<double> Bounds, List<double> Rates) : ILenientRecognitionEntryModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(LenientRecognitionEntry item, double valueValue)
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

public sealed record ChannelLenientRecognitionEntryAdjuster(Dictionary<string, double> Factors) : ILenientRecognitionEntryModifier
{
    public string Describe()
    {
        return "by channel";
    }

    public double Adjust(LenientRecognitionEntry item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the lenient recognition entry steps in order and keeps a trace of each.</summary>
public sealed class LenientRecognitionEntryEngine(List<ILenientRecognitionEntryModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<ILenientRecognitionEntryModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static LenientRecognitionEntryEngine DefaultLenientRecognitionEntryEngine(Registry flags)
    {
        return new LenientRecognitionEntryEngine(
            new List<ILenientRecognitionEntryModifier>
            {
                new RoundingLenientRecognitionEntryAdjuster(5.0),
                new FlaggedLenientRecognitionEntryAdjuster(flags, "lenient-recognition-entry-tiered", -4.2),
                new PercentLenientRecognitionEntryAdjuster(7.5),
                new CapLenientRecognitionEntryAdjuster(29618.0),
                new TieredLenientRecognitionEntryAdjuster(
                    new List<double> { 4966.0, 7856.0, 9848.0 },
                    new List<double> { 0.85, 0.93, 0.96 }),
                new ChannelLenientRecognitionEntryAdjuster(
                    new Dictionary<string, double> { ["west"] = 0.8, ["online"] = 0.78 }),
            },
            new List<string>(),
            2.3,
            43775.2);
    }

    public double Compute(LenientRecognitionEntry item)
    {
        var current = item.EffectiveLoad();
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

    public Dictionary<string, double> EvaluateAll(List<LenientRecognitionEntry> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
