namespace Acme.Workforce.Engagement.Recognition.Claim;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the recognition group calculation.</summary>
public interface IRecognitionGroupModifier
{
    string Describe();
    double Adjust(RecognitionGroup item, double valueValue);
}

public sealed record FlaggedRecognitionGroupAdjuster(Registry Flags, string Flag, double Percent) : IRecognitionGroupModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(RecognitionGroup item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingRecognitionGroupAdjuster(double Step) : IRecognitionGroupModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(RecognitionGroup item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record TitleRecognitionGroupAdjuster(Dictionary<string, double> Factors) : IRecognitionGroupModifier
{
    public string Describe()
    {
        return "by title";
    }

    public double Adjust(RecognitionGroup item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record PercentRecognitionGroupAdjuster(double Percent) : IRecognitionGroupModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(RecognitionGroup item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredRecognitionGroupAdjuster(List<double> Bounds, List<double> Rates) : IRecognitionGroupModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(RecognitionGroup item, double valueValue)
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

/// <summary>Runs the recognition group steps in order and keeps a trace of each.</summary>
public sealed class RecognitionGroupEngine(List<IRecognitionGroupModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IRecognitionGroupModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static RecognitionGroupEngine DefaultRecognitionGroupEngine(Registry flags)
    {
        return new RecognitionGroupEngine(
            new List<IRecognitionGroupModifier>
            {
                new FlaggedRecognitionGroupAdjuster(flags, "recognition-group-regional", 4.7),
                new RoundingRecognitionGroupAdjuster(0.5),
                new TitleRecognitionGroupAdjuster(
                    new Dictionary<string, double> { ["east"] = 1.24, ["online"] = 0.9 }),
                new PercentRecognitionGroupAdjuster(21.7),
                new TieredRecognitionGroupAdjuster(
                    new List<double> { 302.0, 3377.0, 6473.0 },
                    new List<double> { 0.81, 0.87, 1.12 }),
            },
            new List<string>(),
            3.1,
            43889.4);
    }

    public double Price(RecognitionGroup item)
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

    public Dictionary<string, double> EvaluateAll(List<RecognitionGroup> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
