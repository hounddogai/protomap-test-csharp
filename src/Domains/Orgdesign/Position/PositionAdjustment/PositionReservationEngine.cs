namespace Acme.Workforce.Orgdesign.Position.PositionAdjustment;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the position reservation calculation.</summary>
public interface IPositionReservationStep
{
    string Describe();
    double Adjust(PositionReservation item, double valueValue);
}

public sealed record MinimumPositionReservationAdjuster(double Minimum) : IPositionReservationStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(PositionReservation item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record PositionReservationFteOffsetAdjuster(double Weight) : IPositionReservationStep
{
    public string Describe()
    {
        return $"plus fte times {Weight}";
    }

    public double Adjust(PositionReservation item, double valueValue)
    {
        return valueValue + item.Fte * Weight;
    }
}

public sealed record LocalePositionReservationAdjuster(Dictionary<string, double> Factors) : IPositionReservationStep
{
    public string Describe()
    {
        return "by locale";
    }

    public double Adjust(PositionReservation item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record CapPositionReservationAdjuster(double Cap) : IPositionReservationStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(PositionReservation item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record PercentPositionReservationAdjuster(double Percent) : IPositionReservationStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(PositionReservation item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingPositionReservationAdjuster(double Step) : IPositionReservationStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(PositionReservation item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the position reservation steps in order and keeps a trace of each.</summary>
public sealed class PositionReservationEngine(List<IPositionReservationStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IPositionReservationStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static PositionReservationEngine DefaultPositionReservationEngine()
    {
        return new PositionReservationEngine(
            new List<IPositionReservationStep>
            {
                new MinimumPositionReservationAdjuster(18.0),
                new PositionReservationFteOffsetAdjuster(1.36),
                new LocalePositionReservationAdjuster(
                    new Dictionary<string, double> { ["online"] = 0.79, ["central"] = 0.84 }),
                new CapPositionReservationAdjuster(26192.0),
                new PercentPositionReservationAdjuster(-18.8),
                new RoundingPositionReservationAdjuster(0.25),
            },
            new List<string>(),
            2.5,
            72401.2);
    }

    public double Run(PositionReservation item)
    {
        var current = item.EffectiveVolume();
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

    public Dictionary<string, double> EvaluateAll(List<PositionReservation> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
