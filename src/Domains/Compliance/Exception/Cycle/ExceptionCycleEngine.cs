namespace Acme.Workforce.Compliance.Exception.Cycle;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the exception cycle calculation.</summary>
public interface IExceptionCycleFactor
{
    string Describe();
    double Adjust(ExceptionCycle item, double valueValue);
}

public sealed record PercentExceptionCycleAdjuster(double Percent) : IExceptionCycleFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(ExceptionCycle item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingExceptionCycleAdjuster(double Step) : IExceptionCycleFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(ExceptionCycle item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record MinimumExceptionCycleAdjuster(double Minimum) : IExceptionCycleFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(ExceptionCycle item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record LabelExceptionCycleAdjuster(Dictionary<string, double> Factors) : IExceptionCycleFactor
{
    public string Describe()
    {
        return "by label";
    }

    public double Adjust(ExceptionCycle item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record ExceptionCycleMaximumOffsetAdjuster(double Weight) : IExceptionCycleFactor
{
    public string Describe()
    {
        return $"plus maximum times {Weight}";
    }

    public double Adjust(ExceptionCycle item, double valueValue)
    {
        return valueValue + item.Maximum * Weight;
    }
}

public sealed record TieredExceptionCycleAdjuster(List<double> Bounds, List<double> Rates) : IExceptionCycleFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(ExceptionCycle item, double valueValue)
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

/// <summary>Runs the exception cycle steps in order and keeps a trace of each.</summary>
public sealed class ExceptionCycleEngine(List<IExceptionCycleFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IExceptionCycleFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static ExceptionCycleEngine DefaultExceptionCycleEngine()
    {
        return new ExceptionCycleEngine(
            new List<IExceptionCycleFactor>
            {
                new PercentExceptionCycleAdjuster(-3.0),
                new RoundingExceptionCycleAdjuster(5.0),
                new MinimumExceptionCycleAdjuster(40.4),
                new LabelExceptionCycleAdjuster(new Dictionary<string, double> { ["retail"] = 0.79 }),
                new ExceptionCycleMaximumOffsetAdjuster(1.48),
                new TieredExceptionCycleAdjuster(
                    new List<double> { 3249.0, 5598.0, 6206.0 },
                    new List<double> { 0.8, 1.18, 1.09 }),
            },
            new List<string>(),
            4.9,
            34816.5);
    }

    public double Evaluate(ExceptionCycle item)
    {
        var current = item.WeightedLoad();
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

    public Dictionary<string, double> EvaluateAll(List<ExceptionCycle> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
