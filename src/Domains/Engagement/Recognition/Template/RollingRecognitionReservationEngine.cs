namespace Acme.Workforce.Engagement.Recognition.Template;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the rolling recognition reservation calculation.</summary>
public interface IRollingRecognitionReservationAdjuster
{
    string Describe();
    double Adjust(RollingRecognitionReservation item, double valueValue);
}

public sealed record TieredRollingRecognitionReservationAdjuster(List<double> Bounds, List<double> Rates) : IRollingRecognitionReservationAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(RollingRecognitionReservation item, double valueValue)
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

public sealed record RoundingRollingRecognitionReservationAdjuster(double Step) : IRollingRecognitionReservationAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(RollingRecognitionReservation item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record FlaggedRollingRecognitionReservationAdjuster(Registry Flags, string Flag, double Percent) : IRollingRecognitionReservationAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(RollingRecognitionReservation item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record PercentRollingRecognitionReservationAdjuster(double Percent) : IRollingRecognitionReservationAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(RollingRecognitionReservation item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the rolling recognition reservation steps in order and keeps a trace of each.</summary>
public sealed class RollingRecognitionReservationEngine(List<IRollingRecognitionReservationAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IRollingRecognitionReservationAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static RollingRecognitionReservationEngine DefaultRollingRecognitionReservationEngine(Registry flags)
    {
        return new RollingRecognitionReservationEngine(
            new List<IRollingRecognitionReservationAdjuster>
            {
                new TieredRollingRecognitionReservationAdjuster(
                    new List<double> { 4402.0, 4856.0, 4865.0, 8419.0 },
                    new List<double> { 1.04, 0.86, 1.11, 0.9 }),
                new RoundingRollingRecognitionReservationAdjuster(5.0),
                new FlaggedRollingRecognitionReservationAdjuster(
                    flags,
                    "rolling-recognition-reservation-seasonal",
                    7.6),
                new PercentRollingRecognitionReservationAdjuster(-11.2),
            },
            new List<string>(),
            0.1,
            87722.8);
    }

    public double Run(RollingRecognitionReservation item)
    {
        var current = item.NetValue();
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

    public Dictionary<string, double> EvaluateAll(List<RollingRecognitionReservation> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
