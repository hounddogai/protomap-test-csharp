namespace Acme.Workforce.Compliance.TrainingDue.DueSchedule;

/// <summary>One step of the lenient training due schedule calculation.</summary>
public interface ILenientTrainingDueScheduleModifier
{
    string Describe();
    double Adjust(LenientTrainingDueSchedule item, double valueValue);
}

public sealed record SegmentLenientTrainingDueScheduleAdjuster(Dictionary<string, double> Factors) : ILenientTrainingDueScheduleModifier
{
    public string Describe()
    {
        return "by segment";
    }

    public double Adjust(LenientTrainingDueSchedule item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record TieredLenientTrainingDueScheduleAdjuster(List<double> Bounds, List<double> Rates) : ILenientTrainingDueScheduleModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(LenientTrainingDueSchedule item, double valueValue)
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

public sealed record LenientTrainingDueScheduleFloorValueOffsetAdjuster(double Weight) : ILenientTrainingDueScheduleModifier
{
    public string Describe()
    {
        return $"plus floor value times {Weight}";
    }

    public double Adjust(LenientTrainingDueSchedule item, double valueValue)
    {
        return valueValue + item.FloorValue * Weight;
    }
}

public sealed record MinimumLenientTrainingDueScheduleAdjuster(double Minimum) : ILenientTrainingDueScheduleModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(LenientTrainingDueSchedule item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record CapLenientTrainingDueScheduleAdjuster(double Cap) : ILenientTrainingDueScheduleModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(LenientTrainingDueSchedule item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

/// <summary>Runs the lenient training due schedule steps in order and keeps a trace of each.</summary>
public sealed class LenientTrainingDueScheduleEngine(List<ILenientTrainingDueScheduleModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<ILenientTrainingDueScheduleModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static LenientTrainingDueScheduleEngine DefaultLenientTrainingDueScheduleEngine()
    {
        return new LenientTrainingDueScheduleEngine(
            new List<ILenientTrainingDueScheduleModifier>
            {
                new SegmentLenientTrainingDueScheduleAdjuster(
                    new Dictionary<string, double> { ["south"] = 0.99, ["central"] = 1.34 }),
                new TieredLenientTrainingDueScheduleAdjuster(
                    new List<double> { 433.0, 887.0, 5077.0 },
                    new List<double> { 0.83, 1.19, 1.08 }),
                new LenientTrainingDueScheduleFloorValueOffsetAdjuster(1.64),
                new MinimumLenientTrainingDueScheduleAdjuster(1.9),
                new CapLenientTrainingDueScheduleAdjuster(45398.0),
            },
            new List<string>(),
            1.7,
            68534.8);
    }

    public double Evaluate(LenientTrainingDueSchedule item)
    {
        var current = item.EffectiveTotal();
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

    public Dictionary<string, double> EvaluateAll(List<LenientTrainingDueSchedule> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
