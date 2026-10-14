namespace Acme.Workforce.Engagement.Recognition.Agreement;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the recognition agreement calculation.</summary>
public interface IRecognitionAgreementFactor
{
    string Describe();
    double Adjust(RecognitionAgreement item, double valueValue);
}

public sealed record FlaggedRecognitionAgreementAdjuster(Registry Flags, string Flag, double Percent) : IRecognitionAgreementFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(RecognitionAgreement item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record LocaleRecognitionAgreementAdjuster(Dictionary<string, double> Factors) : IRecognitionAgreementFactor
{
    public string Describe()
    {
        return "by locale";
    }

    public double Adjust(RecognitionAgreement item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record RecognitionAgreementThresholdOffsetAdjuster(double Weight) : IRecognitionAgreementFactor
{
    public string Describe()
    {
        return $"plus threshold times {Weight}";
    }

    public double Adjust(RecognitionAgreement item, double valueValue)
    {
        return valueValue + item.Threshold * Weight;
    }
}

public sealed record CapRecognitionAgreementAdjuster(double Cap) : IRecognitionAgreementFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(RecognitionAgreement item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

/// <summary>Runs the recognition agreement steps in order and keeps a trace of each.</summary>
public sealed class RecognitionAgreementEngine(List<IRecognitionAgreementFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IRecognitionAgreementFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static RecognitionAgreementEngine DefaultRecognitionAgreementEngine(Registry flags)
    {
        return new RecognitionAgreementEngine(
            new List<IRecognitionAgreementFactor>
            {
                new FlaggedRecognitionAgreementAdjuster(flags, "recognition-agreement-default", 7.7),
                new LocaleRecognitionAgreementAdjuster(new Dictionary<string, double> { ["west"] = 1.18 }),
                new RecognitionAgreementThresholdOffsetAdjuster(1.45),
                new CapRecognitionAgreementAdjuster(37928.0),
            },
            new List<string>(),
            3.4,
            92572.6);
    }

    public double Compute(RecognitionAgreement item)
    {
        var current = item.EffectiveTotal();
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

    public Dictionary<string, double> EvaluateAll(List<RecognitionAgreement> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
