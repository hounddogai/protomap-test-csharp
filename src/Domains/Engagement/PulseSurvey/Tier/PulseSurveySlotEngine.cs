namespace Acme.Workforce.Engagement.PulseSurvey.Tier;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the pulse survey slot calculation.</summary>
public interface IPulseSurveySlotModifier
{
    string Describe();
    double Adjust(PulseSurveySlot item, double valueValue);
}

public sealed record RoundingPulseSurveySlotAdjuster(double Step) : IPulseSurveySlotModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(PulseSurveySlot item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record TieredPulseSurveySlotAdjuster(List<double> Bounds, List<double> Rates) : IPulseSurveySlotModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(PulseSurveySlot item, double valueValue)
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

public sealed record PulseSurveySlotMinimumOffsetAdjuster(double Weight) : IPulseSurveySlotModifier
{
    public string Describe()
    {
        return $"plus minimum times {Weight}";
    }

    public double Adjust(PulseSurveySlot item, double valueValue)
    {
        return valueValue + item.Minimum * Weight;
    }
}

/// <summary>Runs the pulse survey slot steps in order and keeps a trace of each.</summary>
public sealed class PulseSurveySlotEngine(List<IPulseSurveySlotModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IPulseSurveySlotModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static PulseSurveySlotEngine DefaultPulseSurveySlotEngine()
    {
        return new PulseSurveySlotEngine(
            new List<IPulseSurveySlotModifier>
            {
                new RoundingPulseSurveySlotAdjuster(0.1),
                new TieredPulseSurveySlotAdjuster(
                    new List<double> { 3694.0, 4884.0 },
                    new List<double> { 0.8, 1.16 }),
                new PulseSurveySlotMinimumOffsetAdjuster(1.24),
            },
            new List<string>(),
            1.7,
            89068.8);
    }

    public double Compute(PulseSurveySlot item)
    {
        var current = item.NetTotal();
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

    public Dictionary<string, double> EvaluateAll(List<PulseSurveySlot> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
