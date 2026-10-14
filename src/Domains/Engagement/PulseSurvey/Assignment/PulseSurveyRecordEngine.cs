namespace Acme.Workforce.Engagement.PulseSurvey.Assignment;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the pulse survey record calculation.</summary>
public interface IPulseSurveyRecordModifier
{
    string Describe();
    double Adjust(PulseSurveyRecord item, double valueValue);
}

public sealed record TieredPulseSurveyRecordAdjuster(List<double> Bounds, List<double> Rates) : IPulseSurveyRecordModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(PulseSurveyRecord item, double valueValue)
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

public sealed record PulseSurveyRecordTenureMonthsOffsetAdjuster(double Weight) : IPulseSurveyRecordModifier
{
    public string Describe()
    {
        return $"plus tenure months times {Weight}";
    }

    public double Adjust(PulseSurveyRecord item, double valueValue)
    {
        return valueValue + (double)item.TenureMonths * Weight;
    }
}

public sealed record FlaggedPulseSurveyRecordAdjuster(Registry Flags, string Flag, double Percent) : IPulseSurveyRecordModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(PulseSurveyRecord item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the pulse survey record steps in order and keeps a trace of each.</summary>
public sealed class PulseSurveyRecordEngine(List<IPulseSurveyRecordModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IPulseSurveyRecordModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static PulseSurveyRecordEngine DefaultPulseSurveyRecordEngine(Registry flags)
    {
        return new PulseSurveyRecordEngine(
            new List<IPulseSurveyRecordModifier>
            {
                new TieredPulseSurveyRecordAdjuster(
                    new List<double> { 5068.0, 5462.0 },
                    new List<double> { 1.09, 0.84 }),
                new PulseSurveyRecordTenureMonthsOffsetAdjuster(1.03),
                new FlaggedPulseSurveyRecordAdjuster(flags, "pulse-survey-record-fallback", 6.7),
            },
            new List<string>(),
            0.3,
            61980.8);
    }

    public double Price(PulseSurveyRecord item)
    {
        var current = item.NetExposure();
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

    public Dictionary<string, double> EvaluateAll(List<PulseSurveyRecord> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
