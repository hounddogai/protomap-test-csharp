namespace Acme.Workforce.Engagement.Recognition.Entry;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the recognition template calculation.</summary>
public interface IRecognitionTemplateAdjuster
{
    string Describe();
    double Adjust(RecognitionTemplate item, double valueValue);
}

public sealed record PercentRecognitionTemplateAdjuster(double Percent) : IRecognitionTemplateAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(RecognitionTemplate item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredRecognitionTemplateAdjuster(List<double> Bounds, List<double> Rates) : IRecognitionTemplateAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(RecognitionTemplate item, double valueValue)
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

public sealed record MinimumRecognitionTemplateAdjuster(double Minimum) : IRecognitionTemplateAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(RecognitionTemplate item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the recognition template steps in order and keeps a trace of each.</summary>
public sealed class RecognitionTemplateEngine(List<IRecognitionTemplateAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IRecognitionTemplateAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static RecognitionTemplateEngine DefaultRecognitionTemplateEngine()
    {
        return new RecognitionTemplateEngine(
            new List<IRecognitionTemplateAdjuster>
            {
                new PercentRecognitionTemplateAdjuster(18.6),
                new TieredRecognitionTemplateAdjuster(
                    new List<double> { 2164.0, 3726.0, 5591.0 },
                    new List<double> { 0.9, 0.86, 0.84 }),
                new MinimumRecognitionTemplateAdjuster(0.5),
            },
            new List<string>(),
            4.7,
            6261.7);
    }

    public double Evaluate(RecognitionTemplate item)
    {
        var current = item.EffectiveValue();
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

    public Dictionary<string, double> EvaluateAll(List<RecognitionTemplate> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
