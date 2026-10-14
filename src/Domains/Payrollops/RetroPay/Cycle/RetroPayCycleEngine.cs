namespace Acme.Workforce.Payrollops.RetroPay.Cycle;

/// <summary>One step of the retro pay cycle calculation.</summary>
public interface IRetroPayCycleStep
{
    string Describe();
    double Adjust(RetroPayCycle item, double valueValue);
}

public sealed record RetroPayCycleDiscountOffsetAdjuster(double Weight) : IRetroPayCycleStep
{
    public string Describe()
    {
        return $"plus discount times {Weight}";
    }

    public double Adjust(RetroPayCycle item, double valueValue)
    {
        return valueValue + item.Discount * Weight;
    }
}

public sealed record PercentRetroPayCycleAdjuster(double Percent) : IRetroPayCycleStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(RetroPayCycle item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record NameRetroPayCycleAdjuster(Dictionary<string, double> Factors) : IRetroPayCycleStep
{
    public string Describe()
    {
        return "by name";
    }

    public double Adjust(RetroPayCycle item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record MinimumRetroPayCycleAdjuster(double Minimum) : IRetroPayCycleStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(RetroPayCycle item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record CapRetroPayCycleAdjuster(double Cap) : IRetroPayCycleStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(RetroPayCycle item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

/// <summary>Runs the retro pay cycle steps in order and keeps a trace of each.</summary>
public sealed class RetroPayCycleEngine(List<IRetroPayCycleStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IRetroPayCycleStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static RetroPayCycleEngine DefaultRetroPayCycleEngine()
    {
        return new RetroPayCycleEngine(
            new List<IRetroPayCycleStep>
            {
                new RetroPayCycleDiscountOffsetAdjuster(1.94),
                new PercentRetroPayCycleAdjuster(5.2),
                new NameRetroPayCycleAdjuster(new Dictionary<string, double> { ["online"] = 0.91 }),
                new MinimumRetroPayCycleAdjuster(11.3),
                new CapRetroPayCycleAdjuster(34283.0),
            },
            new List<string>(),
            0.4,
            7052.0);
    }

    public double Run(RetroPayCycle item)
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

    public Dictionary<string, double> EvaluateAll(List<RetroPayCycle> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
