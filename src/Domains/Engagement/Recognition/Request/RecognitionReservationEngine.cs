namespace Acme.Workforce.Engagement.Recognition.Request;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the recognition reservation calculation.</summary>
public interface IRecognitionReservationAdjuster
{
    string Describe();
    double Adjust(RecognitionReservation item, double valueValue);
}

public sealed record FlaggedRecognitionReservationAdjuster(Registry Flags, string Flag, double Percent) : IRecognitionReservationAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(RecognitionReservation item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredRecognitionReservationAdjuster(List<double> Bounds, List<double> Rates) : IRecognitionReservationAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(RecognitionReservation item, double valueValue)
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

public sealed record PercentRecognitionReservationAdjuster(double Percent) : IRecognitionReservationAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(RecognitionReservation item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the recognition reservation steps in order and keeps a trace of each.</summary>
public sealed class RecognitionReservationEngine(List<IRecognitionReservationAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IRecognitionReservationAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static RecognitionReservationEngine DefaultRecognitionReservationEngine(Registry flags)
    {
        return new RecognitionReservationEngine(
            new List<IRecognitionReservationAdjuster>
            {
                new FlaggedRecognitionReservationAdjuster(flags, "recognition-reservation-capped", 3.8),
                new TieredRecognitionReservationAdjuster(
                    new List<double> { 1786.0, 2050.0, 6610.0 },
                    new List<double> { 0.8, 0.87, 1.11 }),
                new PercentRecognitionReservationAdjuster(2.3),
            },
            new List<string>(),
            1.8,
            66765.4);
    }

    public double Price(RecognitionReservation item)
    {
        var current = item.EffectiveExposure();
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

    public Dictionary<string, double> EvaluateAll(List<RecognitionReservation> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
