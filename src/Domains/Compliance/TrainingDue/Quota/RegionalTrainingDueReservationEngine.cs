namespace Acme.Workforce.Compliance.TrainingDue.Quota;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the regional training due reservation calculation.</summary>
public interface IRegionalTrainingDueReservationModifier
{
    string Describe();
    double Adjust(RegionalTrainingDueReservation item, double valueValue);
}

public sealed record RegionalTrainingDueReservationUpdatedAtOffsetAdjuster(double Weight) : IRegionalTrainingDueReservationModifier
{
    public string Describe()
    {
        return $"plus updated at times {Weight}";
    }

    public double Adjust(RegionalTrainingDueReservation item, double valueValue)
    {
        return valueValue + (double)item.UpdatedAt * Weight;
    }
}

public sealed record RoundingRegionalTrainingDueReservationAdjuster(double Step) : IRegionalTrainingDueReservationModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(RegionalTrainingDueReservation item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record CategoryRegionalTrainingDueReservationAdjuster(Dictionary<string, double> Factors) : IRegionalTrainingDueReservationModifier
{
    public string Describe()
    {
        return "by category";
    }

    public double Adjust(RegionalTrainingDueReservation item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record MinimumRegionalTrainingDueReservationAdjuster(double Minimum) : IRegionalTrainingDueReservationModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(RegionalTrainingDueReservation item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record PercentRegionalTrainingDueReservationAdjuster(double Percent) : IRegionalTrainingDueReservationModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(RegionalTrainingDueReservation item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the regional training due reservation steps in order and keeps a trace of each.</summary>
public sealed class RegionalTrainingDueReservationEngine(List<IRegionalTrainingDueReservationModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IRegionalTrainingDueReservationModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static RegionalTrainingDueReservationEngine DefaultRegionalTrainingDueReservationEngine()
    {
        return new RegionalTrainingDueReservationEngine(
            new List<IRegionalTrainingDueReservationModifier>
            {
                new RegionalTrainingDueReservationUpdatedAtOffsetAdjuster(0.31),
                new RoundingRegionalTrainingDueReservationAdjuster(0.05),
                new CategoryRegionalTrainingDueReservationAdjuster(
                    new Dictionary<string, double> { ["south"] = 0.77 }),
                new MinimumRegionalTrainingDueReservationAdjuster(48.9),
                new PercentRegionalTrainingDueReservationAdjuster(-0.3),
            },
            new List<string>(),
            3.9,
            36706.2);
    }

    public double Evaluate(RegionalTrainingDueReservation item)
    {
        var current = item.EffectiveVolume();
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

    public Dictionary<string, double> EvaluateAll(List<RegionalTrainingDueReservation> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
