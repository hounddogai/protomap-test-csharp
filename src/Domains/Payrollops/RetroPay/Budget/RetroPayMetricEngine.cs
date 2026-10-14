namespace Acme.Workforce.Payrollops.RetroPay.Budget;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the retro pay metric calculation.</summary>
public interface IRetroPayMetricModifier
{
    string Describe();
    double Adjust(RetroPayMetric item, double valueValue);
}

public sealed record RoundingRetroPayMetricAdjuster(double Step) : IRetroPayMetricModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(RetroPayMetric item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record TitleRetroPayMetricAdjuster(Dictionary<string, double> Factors) : IRetroPayMetricModifier
{
    public string Describe()
    {
        return "by title";
    }

    public double Adjust(RetroPayMetric item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record TieredRetroPayMetricAdjuster(List<double> Bounds, List<double> Rates) : IRetroPayMetricModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(RetroPayMetric item, double valueValue)
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

public sealed record CapRetroPayMetricAdjuster(double Cap) : IRetroPayMetricModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(RetroPayMetric item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record RetroPayMetricMinimumOffsetAdjuster(double Weight) : IRetroPayMetricModifier
{
    public string Describe()
    {
        return $"plus minimum times {Weight}";
    }

    public double Adjust(RetroPayMetric item, double valueValue)
    {
        return valueValue + item.Minimum * Weight;
    }
}

public sealed record MinimumRetroPayMetricAdjuster(double Minimum) : IRetroPayMetricModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(RetroPayMetric item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the retro pay metric steps in order and keeps a trace of each.</summary>
public sealed class RetroPayMetricEngine(List<IRetroPayMetricModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IRetroPayMetricModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static RetroPayMetricEngine DefaultRetroPayMetricEngine()
    {
        return new RetroPayMetricEngine(
            new List<IRetroPayMetricModifier>
            {
                new RoundingRetroPayMetricAdjuster(0.05),
                new TitleRetroPayMetricAdjuster(
                    new Dictionary<string, double> { ["retail"] = 0.91, ["online"] = 1.39 }),
                new TieredRetroPayMetricAdjuster(new List<double> { 1401.0, 7322.0 }, new List<double> { 0.8, 1.2 }),
                new CapRetroPayMetricAdjuster(14659.0),
                new RetroPayMetricMinimumOffsetAdjuster(0.72),
                new MinimumRetroPayMetricAdjuster(36.1),
            },
            new List<string>(),
            0.9,
            71253.2);
    }

    public double Run(RetroPayMetric item)
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

    public Dictionary<string, double> EvaluateAll(List<RetroPayMetric> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
