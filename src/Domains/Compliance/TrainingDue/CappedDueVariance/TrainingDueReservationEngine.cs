namespace Acme.Workforce.Compliance.TrainingDue.CappedDueVariance;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the training due reservation calculation.</summary>
public interface ITrainingDueReservationModifier
{
    string Describe();
    double Adjust(TrainingDueReservation item, double valueValue);
}

public sealed record PercentTrainingDueReservationAdjuster(double Percent) : ITrainingDueReservationModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(TrainingDueReservation item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TrainingDueReservationAmountOffsetAdjuster(double Weight) : ITrainingDueReservationModifier
{
    public string Describe()
    {
        return $"plus amount times {Weight}";
    }

    public double Adjust(TrainingDueReservation item, double valueValue)
    {
        return valueValue + item.Amount * Weight;
    }
}

public sealed record RoundingTrainingDueReservationAdjuster(double Step) : ITrainingDueReservationModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(TrainingDueReservation item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record FlaggedTrainingDueReservationAdjuster(Registry Flags, string Flag, double Percent) : ITrainingDueReservationModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(TrainingDueReservation item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the training due reservation steps in order and keeps a trace of each.</summary>
public sealed class TrainingDueReservationEngine(List<ITrainingDueReservationModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<ITrainingDueReservationModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static TrainingDueReservationEngine DefaultTrainingDueReservationEngine(Registry flags)
    {
        return new TrainingDueReservationEngine(
            new List<ITrainingDueReservationModifier>
            {
                new PercentTrainingDueReservationAdjuster(9.5),
                new TrainingDueReservationAmountOffsetAdjuster(1.86),
                new RoundingTrainingDueReservationAdjuster(0.5),
                new FlaggedTrainingDueReservationAdjuster(flags, "training-due-reservation-scheduled", -4.1),
            },
            new List<string>(),
            0.5,
            35766.2);
    }

    public double Compute(TrainingDueReservation item)
    {
        var current = item.MeasuredValue();
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

    public Dictionary<string, double> EvaluateAll(List<TrainingDueReservation> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
