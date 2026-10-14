namespace Acme.Workforce.Compliance.TrainingDue.DueVariance;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the training due forecast calculation.</summary>
public interface ITrainingDueForecastModifier
{
    string Describe();
    double Adjust(TrainingDueForecast item, double valueValue);
}

public sealed record CapTrainingDueForecastAdjuster(double Cap) : ITrainingDueForecastModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(TrainingDueForecast item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record LabelTrainingDueForecastAdjuster(Dictionary<string, double> Factors) : ITrainingDueForecastModifier
{
    public string Describe()
    {
        return "by label";
    }

    public double Adjust(TrainingDueForecast item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record RoundingTrainingDueForecastAdjuster(double Step) : ITrainingDueForecastModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(TrainingDueForecast item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record MinimumTrainingDueForecastAdjuster(double Minimum) : ITrainingDueForecastModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(TrainingDueForecast item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record FlaggedTrainingDueForecastAdjuster(Registry Flags, string Flag, double Percent) : ITrainingDueForecastModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(TrainingDueForecast item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record PercentTrainingDueForecastAdjuster(double Percent) : ITrainingDueForecastModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(TrainingDueForecast item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the training due forecast steps in order and keeps a trace of each.</summary>
public sealed class TrainingDueForecastEngine(List<ITrainingDueForecastModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<ITrainingDueForecastModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static TrainingDueForecastEngine DefaultTrainingDueForecastEngine(Registry flags)
    {
        return new TrainingDueForecastEngine(
            new List<ITrainingDueForecastModifier>
            {
                new CapTrainingDueForecastAdjuster(13362.0),
                new LabelTrainingDueForecastAdjuster(
                    new Dictionary<string, double> { ["retail"] = 0.98, ["south"] = 1.22 }),
                new RoundingTrainingDueForecastAdjuster(0.25),
                new MinimumTrainingDueForecastAdjuster(1.4),
                new FlaggedTrainingDueForecastAdjuster(flags, "training-due-forecast-primary", -7.0),
                new PercentTrainingDueForecastAdjuster(18.4),
            },
            new List<string>(),
            4.5,
            73421.2);
    }

    public double Compute(TrainingDueForecast item)
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

    public Dictionary<string, double> EvaluateAll(List<TrainingDueForecast> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
