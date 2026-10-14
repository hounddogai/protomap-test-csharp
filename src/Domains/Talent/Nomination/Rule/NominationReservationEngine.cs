namespace Acme.Workforce.Talent.Nomination.Rule;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the nomination reservation calculation.</summary>
public interface INominationReservationModifier
{
    string Describe();
    double Adjust(NominationReservation item, double valueValue);
}

public sealed record CapNominationReservationAdjuster(double Cap) : INominationReservationModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(NominationReservation item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record SegmentNominationReservationAdjuster(Dictionary<string, double> Factors) : INominationReservationModifier
{
    public string Describe()
    {
        return "by segment";
    }

    public double Adjust(NominationReservation item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record RoundingNominationReservationAdjuster(double Step) : INominationReservationModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(NominationReservation item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the nomination reservation steps in order and keeps a trace of each.</summary>
public sealed class NominationReservationEngine(List<INominationReservationModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<INominationReservationModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static NominationReservationEngine DefaultNominationReservationEngine()
    {
        return new NominationReservationEngine(
            new List<INominationReservationModifier>
            {
                new CapNominationReservationAdjuster(28829.0),
                new SegmentNominationReservationAdjuster(
                    new Dictionary<string, double> { ["west"] = 1.36, ["online"] = 1.31 }),
                new RoundingNominationReservationAdjuster(5.0),
            },
            new List<string>(),
            2.3,
            41233.4);
    }

    public double Run(NominationReservation item)
    {
        var current = item.NetVolume();
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

    public Dictionary<string, double> EvaluateAll(List<NominationReservation> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
