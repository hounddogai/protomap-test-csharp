namespace Acme.Workforce.Engagement.Sentiment.Exception;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the global sentiment bundle calculation.</summary>
public interface IGlobalSentimentBundleStep
{
    string Describe();
    double Adjust(GlobalSentimentBundle item, double valueValue);
}

public sealed record FlaggedGlobalSentimentBundleAdjuster(Registry Flags, string Flag, double Percent) : IGlobalSentimentBundleStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(GlobalSentimentBundle item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record PercentGlobalSentimentBundleAdjuster(double Percent) : IGlobalSentimentBundleStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(GlobalSentimentBundle item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumGlobalSentimentBundleAdjuster(double Minimum) : IGlobalSentimentBundleStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(GlobalSentimentBundle item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the global sentiment bundle steps in order and keeps a trace of each.</summary>
public sealed class GlobalSentimentBundleEngine(List<IGlobalSentimentBundleStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IGlobalSentimentBundleStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static GlobalSentimentBundleEngine DefaultGlobalSentimentBundleEngine(Registry flags)
    {
        return new GlobalSentimentBundleEngine(
            new List<IGlobalSentimentBundleStep>
            {
                new FlaggedGlobalSentimentBundleAdjuster(flags, "global-sentiment-bundle-weighted", -8.5),
                new PercentGlobalSentimentBundleAdjuster(16.7),
                new MinimumGlobalSentimentBundleAdjuster(11.1),
            },
            new List<string>(),
            0.7,
            48572.2);
    }

    public double Evaluate(GlobalSentimentBundle item)
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

    public Dictionary<string, double> EvaluateAll(List<GlobalSentimentBundle> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
