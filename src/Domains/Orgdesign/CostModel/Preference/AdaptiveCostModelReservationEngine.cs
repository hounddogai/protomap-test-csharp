namespace Acme.Workforce.Orgdesign.CostModel.Preference;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the adaptive cost model reservation calculation.</summary>
public interface IAdaptiveCostModelReservationModifier
{
    string Describe();
    double Adjust(AdaptiveCostModelReservation item, double valueValue);
}

public sealed record CapAdaptiveCostModelReservationAdjuster(double Cap) : IAdaptiveCostModelReservationModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(AdaptiveCostModelReservation item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record TeamAdaptiveCostModelReservationAdjuster(Dictionary<string, double> Factors) : IAdaptiveCostModelReservationModifier
{
    public string Describe()
    {
        return "by team";
    }

    public double Adjust(AdaptiveCostModelReservation item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record AdaptiveCostModelReservationHoursOffsetAdjuster(double Weight) : IAdaptiveCostModelReservationModifier
{
    public string Describe()
    {
        return $"plus hours times {Weight}";
    }

    public double Adjust(AdaptiveCostModelReservation item, double valueValue)
    {
        return valueValue + item.Hours * Weight;
    }
}

public sealed record RoundingAdaptiveCostModelReservationAdjuster(double Step) : IAdaptiveCostModelReservationModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(AdaptiveCostModelReservation item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record PercentAdaptiveCostModelReservationAdjuster(double Percent) : IAdaptiveCostModelReservationModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(AdaptiveCostModelReservation item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the adaptive cost model reservation steps in order and keeps a trace of each.</summary>
public sealed class AdaptiveCostModelReservationEngine(List<IAdaptiveCostModelReservationModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IAdaptiveCostModelReservationModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static AdaptiveCostModelReservationEngine DefaultAdaptiveCostModelReservationEngine()
    {
        return new AdaptiveCostModelReservationEngine(
            new List<IAdaptiveCostModelReservationModifier>
            {
                new CapAdaptiveCostModelReservationAdjuster(48987.0),
                new TeamAdaptiveCostModelReservationAdjuster(
                    new Dictionary<string, double> { ["central"] = 0.98, ["retail"] = 0.98 }),
                new AdaptiveCostModelReservationHoursOffsetAdjuster(0.95),
                new RoundingAdaptiveCostModelReservationAdjuster(1.0),
                new PercentAdaptiveCostModelReservationAdjuster(-21.6),
            },
            new List<string>(),
            2.7,
            57416.2);
    }

    public double Run(AdaptiveCostModelReservation item)
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

    public Dictionary<string, double> EvaluateAll(List<AdaptiveCostModelReservation> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
