namespace Acme.Workforce.Engagement.Recognition.Ledger;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the recognition baseline calculation.</summary>
public interface IRecognitionBaselineModifier
{
    string Describe();
    double Adjust(RecognitionBaseline item, double valueValue);
}

public sealed record MinimumRecognitionBaselineAdjuster(double Minimum) : IRecognitionBaselineModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(RecognitionBaseline item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record PercentRecognitionBaselineAdjuster(double Percent) : IRecognitionBaselineModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(RecognitionBaseline item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record FlaggedRecognitionBaselineAdjuster(Registry Flags, string Flag, double Percent) : IRecognitionBaselineModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(RecognitionBaseline item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record NameRecognitionBaselineAdjuster(Dictionary<string, double> Factors) : IRecognitionBaselineModifier
{
    public string Describe()
    {
        return "by name";
    }

    public double Adjust(RecognitionBaseline item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record TieredRecognitionBaselineAdjuster(List<double> Bounds, List<double> Rates) : IRecognitionBaselineModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(RecognitionBaseline item, double valueValue)
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

/// <summary>Runs the recognition baseline steps in order and keeps a trace of each.</summary>
public sealed class RecognitionBaselineEngine(List<IRecognitionBaselineModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IRecognitionBaselineModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static RecognitionBaselineEngine DefaultRecognitionBaselineEngine(Registry flags)
    {
        return new RecognitionBaselineEngine(
            new List<IRecognitionBaselineModifier>
            {
                new MinimumRecognitionBaselineAdjuster(16.0),
                new PercentRecognitionBaselineAdjuster(-16.6),
                new FlaggedRecognitionBaselineAdjuster(flags, "recognition-baseline-strict", -1.3),
                new NameRecognitionBaselineAdjuster(new Dictionary<string, double> { ["east"] = 1.13 }),
                new TieredRecognitionBaselineAdjuster(
                    new List<double> { 6071.0, 6894.0 },
                    new List<double> { 0.92, 0.99 }),
            },
            new List<string>(),
            3.6,
            39300.4);
    }

    public double Price(RecognitionBaseline item)
    {
        var current = item.EffectiveValue();
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

    public Dictionary<string, double> EvaluateAll(List<RecognitionBaseline> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
