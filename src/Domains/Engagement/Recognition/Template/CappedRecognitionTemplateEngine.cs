namespace Acme.Workforce.Engagement.Recognition.Template;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the capped recognition template calculation.</summary>
public interface ICappedRecognitionTemplateAdjuster
{
    string Describe();
    double Adjust(CappedRecognitionTemplate item, double valueValue);
}

public sealed record RegionCappedRecognitionTemplateAdjuster(Dictionary<string, double> Factors) : ICappedRecognitionTemplateAdjuster
{
    public string Describe()
    {
        return "by region";
    }

    public double Adjust(CappedRecognitionTemplate item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record TieredCappedRecognitionTemplateAdjuster(List<double> Bounds, List<double> Rates) : ICappedRecognitionTemplateAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(CappedRecognitionTemplate item, double valueValue)
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

public sealed record RoundingCappedRecognitionTemplateAdjuster(double Step) : ICappedRecognitionTemplateAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(CappedRecognitionTemplate item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record CappedRecognitionTemplateWeightOffsetAdjuster(double Weight) : ICappedRecognitionTemplateAdjuster
{
    public string Describe()
    {
        return $"plus weight times {Weight}";
    }

    public double Adjust(CappedRecognitionTemplate item, double valueValue)
    {
        return valueValue + item.Weight * Weight;
    }
}

/// <summary>Runs the capped recognition template steps in order and keeps a trace of each.</summary>
public sealed class CappedRecognitionTemplateEngine(List<ICappedRecognitionTemplateAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<ICappedRecognitionTemplateAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static CappedRecognitionTemplateEngine DefaultCappedRecognitionTemplateEngine()
    {
        return new CappedRecognitionTemplateEngine(
            new List<ICappedRecognitionTemplateAdjuster>
            {
                new RegionCappedRecognitionTemplateAdjuster(
                    new Dictionary<string, double> { ["central"] = 1.29, ["south"] = 0.84 }),
                new TieredCappedRecognitionTemplateAdjuster(
                    new List<double> { 3702.0, 5075.0, 7685.0, 9225.0 },
                    new List<double> { 0.96, 1.02, 1.2, 0.86 }),
                new RoundingCappedRecognitionTemplateAdjuster(0.05),
                new CappedRecognitionTemplateWeightOffsetAdjuster(1.71),
            },
            new List<string>(),
            3.5,
            82272.9);
    }

    public double Price(CappedRecognitionTemplate item)
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

    public Dictionary<string, double> EvaluateAll(List<CappedRecognitionTemplate> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
