namespace Acme.Workforce.Engagement.Recognition.OverrideItem;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the recognition bundle calculation.</summary>
public interface IRecognitionBundleModifier
{
    string Describe();
    double Adjust(RecognitionBundle item, double valueValue);
}

public sealed record FlaggedRecognitionBundleAdjuster(Registry Flags, string Flag, double Percent) : IRecognitionBundleModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(RecognitionBundle item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record PercentRecognitionBundleAdjuster(double Percent) : IRecognitionBundleModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(RecognitionBundle item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredRecognitionBundleAdjuster(List<double> Bounds, List<double> Rates) : IRecognitionBundleModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(RecognitionBundle item, double valueValue)
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

public sealed record RecognitionBundleEndsAtOffsetAdjuster(double Weight) : IRecognitionBundleModifier
{
    public string Describe()
    {
        return $"plus ends at times {Weight}";
    }

    public double Adjust(RecognitionBundle item, double valueValue)
    {
        return valueValue + (double)item.EndsAt * Weight;
    }
}

/// <summary>Runs the recognition bundle steps in order and keeps a trace of each.</summary>
public sealed class RecognitionBundleEngine(List<IRecognitionBundleModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IRecognitionBundleModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static RecognitionBundleEngine DefaultRecognitionBundleEngine(Registry flags)
    {
        return new RecognitionBundleEngine(
            new List<IRecognitionBundleModifier>
            {
                new FlaggedRecognitionBundleAdjuster(flags, "recognition-bundle-primary", 7.6),
                new PercentRecognitionBundleAdjuster(14.9),
                new TieredRecognitionBundleAdjuster(
                    new List<double> { 3579.0, 6395.0, 9574.0 },
                    new List<double> { 1.18, 1.05, 0.85 }),
                new RecognitionBundleEndsAtOffsetAdjuster(1.47),
            },
            new List<string>(),
            4.2,
            79341.7);
    }

    public double Compute(RecognitionBundle item)
    {
        var current = item.AdjustedVolume();
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

    public Dictionary<string, double> EvaluateAll(List<RecognitionBundle> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
