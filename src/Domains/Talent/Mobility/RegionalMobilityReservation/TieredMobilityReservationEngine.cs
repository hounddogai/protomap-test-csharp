namespace Acme.Workforce.Talent.Mobility.RegionalMobilityReservation;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the tiered mobility reservation calculation.</summary>
public interface ITieredMobilityReservationModifier
{
    string Describe();
    double Adjust(TieredMobilityReservation item, double valueValue);
}

public sealed record TieredMobilityReservationMaximumOffsetAdjuster(double Weight) : ITieredMobilityReservationModifier
{
    public string Describe()
    {
        return $"plus maximum times {Weight}";
    }

    public double Adjust(TieredMobilityReservation item, double valueValue)
    {
        return valueValue + item.Maximum * Weight;
    }
}

public sealed record RoundingTieredMobilityReservationAdjuster(double Step) : ITieredMobilityReservationModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(TieredMobilityReservation item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record CapTieredMobilityReservationAdjuster(double Cap) : ITieredMobilityReservationModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(TieredMobilityReservation item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

/// <summary>Runs the tiered mobility reservation steps in order and keeps a trace of each.</summary>
public sealed class TieredMobilityReservationEngine(List<ITieredMobilityReservationModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<ITieredMobilityReservationModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static TieredMobilityReservationEngine DefaultTieredMobilityReservationEngine()
    {
        return new TieredMobilityReservationEngine(
            new List<ITieredMobilityReservationModifier>
            {
                new TieredMobilityReservationMaximumOffsetAdjuster(0.34),
                new RoundingTieredMobilityReservationAdjuster(0.1),
                new CapTieredMobilityReservationAdjuster(11268.0),
            },
            new List<string>(),
            2.7,
            95357.4);
    }

    public double Evaluate(TieredMobilityReservation item)
    {
        var current = item.EffectiveLoad();
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

    public Dictionary<string, double> EvaluateAll(List<TieredMobilityReservation> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
