namespace Acme.Workforce.Engagement.PulseSurvey.Group;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the pulse survey reservation calculation.</summary>
public interface IPulseSurveyReservationAdjuster
{
    string Describe();
    double Adjust(PulseSurveyReservation item, double valueValue);
}

public sealed record FlaggedPulseSurveyReservationAdjuster(Registry Flags, string Flag, double Percent) : IPulseSurveyReservationAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(PulseSurveyReservation item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record PercentPulseSurveyReservationAdjuster(double Percent) : IPulseSurveyReservationAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(PulseSurveyReservation item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredPulseSurveyReservationAdjuster(List<double> Bounds, List<double> Rates) : IPulseSurveyReservationAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(PulseSurveyReservation item, double valueValue)
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

public sealed record PulseSurveyReservationFteOffsetAdjuster(double Weight) : IPulseSurveyReservationAdjuster
{
    public string Describe()
    {
        return $"plus fte times {Weight}";
    }

    public double Adjust(PulseSurveyReservation item, double valueValue)
    {
        return valueValue + item.Fte * Weight;
    }
}

/// <summary>Runs the pulse survey reservation steps in order and keeps a trace of each.</summary>
public sealed class PulseSurveyReservationEngine(List<IPulseSurveyReservationAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IPulseSurveyReservationAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static PulseSurveyReservationEngine DefaultPulseSurveyReservationEngine(Registry flags)
    {
        return new PulseSurveyReservationEngine(
            new List<IPulseSurveyReservationAdjuster>
            {
                new FlaggedPulseSurveyReservationAdjuster(flags, "pulse-survey-reservation-capped", 8.1),
                new PercentPulseSurveyReservationAdjuster(7.2),
                new TieredPulseSurveyReservationAdjuster(
                    new List<double> { 2348.0, 9000.0 },
                    new List<double> { 0.84, 1.14 }),
                new PulseSurveyReservationFteOffsetAdjuster(0.2),
            },
            new List<string>(),
            1.6,
            11611.1);
    }

    public double Compute(PulseSurveyReservation item)
    {
        var current = item.AdjustedLoad();
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

    public Dictionary<string, double> EvaluateAll(List<PulseSurveyReservation> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
