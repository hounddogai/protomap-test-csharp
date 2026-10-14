namespace Acme.Workforce.Orgdesign.Span.TieredSpanDraft;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the span manifest calculation.</summary>
public interface ISpanManifestStep
{
    string Describe();
    double Adjust(SpanManifest item, double valueValue);
}

public sealed record RegionSpanManifestAdjuster(Dictionary<string, double> Factors) : ISpanManifestStep
{
    public string Describe()
    {
        return "by region";
    }

    public double Adjust(SpanManifest item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record TieredSpanManifestAdjuster(List<double> Bounds, List<double> Rates) : ISpanManifestStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(SpanManifest item, double valueValue)
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

public sealed record RoundingSpanManifestAdjuster(double Step) : ISpanManifestStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(SpanManifest item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record PercentSpanManifestAdjuster(double Percent) : ISpanManifestStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(SpanManifest item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the span manifest steps in order and keeps a trace of each.</summary>
public sealed class SpanManifestEngine(List<ISpanManifestStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<ISpanManifestStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static SpanManifestEngine DefaultSpanManifestEngine()
    {
        return new SpanManifestEngine(
            new List<ISpanManifestStep>
            {
                new RegionSpanManifestAdjuster(
                    new Dictionary<string, double> { ["north"] = 1.28, ["retail"] = 0.85 }),
                new TieredSpanManifestAdjuster(new List<double> { 8011.0, 8290.0 }, new List<double> { 1.11, 1.0 }),
                new RoundingSpanManifestAdjuster(0.1),
                new PercentSpanManifestAdjuster(14.3),
            },
            new List<string>(),
            4.0,
            63650.7);
    }

    public double Compute(SpanManifest item)
    {
        var current = item.EffectiveExposure();
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

    public Dictionary<string, double> EvaluateAll(List<SpanManifest> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
