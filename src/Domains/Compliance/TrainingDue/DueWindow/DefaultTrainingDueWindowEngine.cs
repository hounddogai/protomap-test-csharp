namespace Acme.Workforce.Compliance.TrainingDue.DueWindow;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the default training due window calculation.</summary>
public interface IDefaultTrainingDueWindowStep
{
    string Describe();
    double Adjust(DefaultTrainingDueWindow item, double valueValue);
}

public sealed record DefaultTrainingDueWindowRevisionOffsetAdjuster(double Weight) : IDefaultTrainingDueWindowStep
{
    public string Describe()
    {
        return $"plus revision times {Weight}";
    }

    public double Adjust(DefaultTrainingDueWindow item, double valueValue)
    {
        return valueValue + (double)item.Revision * Weight;
    }
}

public sealed record CapDefaultTrainingDueWindowAdjuster(double Cap) : IDefaultTrainingDueWindowStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(DefaultTrainingDueWindow item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record PercentDefaultTrainingDueWindowAdjuster(double Percent) : IDefaultTrainingDueWindowStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(DefaultTrainingDueWindow item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredDefaultTrainingDueWindowAdjuster(List<double> Bounds, List<double> Rates) : IDefaultTrainingDueWindowStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(DefaultTrainingDueWindow item, double valueValue)
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

public sealed record RoundingDefaultTrainingDueWindowAdjuster(double Step) : IDefaultTrainingDueWindowStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(DefaultTrainingDueWindow item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record MinimumDefaultTrainingDueWindowAdjuster(double Minimum) : IDefaultTrainingDueWindowStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(DefaultTrainingDueWindow item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the default training due window steps in order and keeps a trace of each.</summary>
public sealed class DefaultTrainingDueWindowEngine(List<IDefaultTrainingDueWindowStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IDefaultTrainingDueWindowStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static DefaultTrainingDueWindowEngine DefaultDefaultTrainingDueWindowEngine()
    {
        return new DefaultTrainingDueWindowEngine(
            new List<IDefaultTrainingDueWindowStep>
            {
                new DefaultTrainingDueWindowRevisionOffsetAdjuster(0.15),
                new CapDefaultTrainingDueWindowAdjuster(32855.0),
                new PercentDefaultTrainingDueWindowAdjuster(-19.0),
                new TieredDefaultTrainingDueWindowAdjuster(
                    new List<double> { 3903.0, 9304.0 },
                    new List<double> { 1.02, 1.01 }),
                new RoundingDefaultTrainingDueWindowAdjuster(0.25),
                new MinimumDefaultTrainingDueWindowAdjuster(3.3),
            },
            new List<string>(),
            1.0,
            7824.4);
    }

    public double Compute(DefaultTrainingDueWindow item)
    {
        var current = item.MeasuredValue();
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

    public Dictionary<string, double> EvaluateAll(List<DefaultTrainingDueWindow> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
