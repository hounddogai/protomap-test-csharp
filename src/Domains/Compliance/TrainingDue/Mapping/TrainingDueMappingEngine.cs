namespace Acme.Workforce.Compliance.TrainingDue.Mapping;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the training due mapping calculation.</summary>
public interface ITrainingDueMappingModifier
{
    string Describe();
    double Adjust(TrainingDueMapping item, double valueValue);
}

public sealed record SegmentTrainingDueMappingAdjuster(Dictionary<string, double> Factors) : ITrainingDueMappingModifier
{
    public string Describe()
    {
        return "by segment";
    }

    public double Adjust(TrainingDueMapping item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record PercentTrainingDueMappingAdjuster(double Percent) : ITrainingDueMappingModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(TrainingDueMapping item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record FlaggedTrainingDueMappingAdjuster(Registry Flags, string Flag, double Percent) : ITrainingDueMappingModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(TrainingDueMapping item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapTrainingDueMappingAdjuster(double Cap) : ITrainingDueMappingModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(TrainingDueMapping item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record MinimumTrainingDueMappingAdjuster(double Minimum) : ITrainingDueMappingModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(TrainingDueMapping item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the training due mapping steps in order and keeps a trace of each.</summary>
public sealed class TrainingDueMappingEngine(List<ITrainingDueMappingModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<ITrainingDueMappingModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static TrainingDueMappingEngine DefaultTrainingDueMappingEngine(Registry flags)
    {
        return new TrainingDueMappingEngine(
            new List<ITrainingDueMappingModifier>
            {
                new SegmentTrainingDueMappingAdjuster(
                    new Dictionary<string, double> { ["retail"] = 0.99, ["online"] = 0.77 }),
                new PercentTrainingDueMappingAdjuster(12.5),
                new FlaggedTrainingDueMappingAdjuster(flags, "training-due-mapping-default", -7.5),
                new CapTrainingDueMappingAdjuster(26641.0),
                new MinimumTrainingDueMappingAdjuster(36.0),
            },
            new List<string>(),
            4.7,
            78186.7);
    }

    public double Compute(TrainingDueMapping item)
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

    public Dictionary<string, double> EvaluateAll(List<TrainingDueMapping> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
