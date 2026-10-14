namespace Acme.Workforce.Payrollops.RetroPay.Threshold;

/// <summary>One step of the retro pay score calculation.</summary>
public interface IRetroPayScoreModifier
{
    string Describe();
    double Adjust(RetroPayScore item, double valueValue);
}

public sealed record RetroPayScoreCapacityOffsetAdjuster(double Weight) : IRetroPayScoreModifier
{
    public string Describe()
    {
        return $"plus capacity times {Weight}";
    }

    public double Adjust(RetroPayScore item, double valueValue)
    {
        return valueValue + (double)item.Capacity * Weight;
    }
}

public sealed record MinimumRetroPayScoreAdjuster(double Minimum) : IRetroPayScoreModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(RetroPayScore item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record TieredRetroPayScoreAdjuster(List<double> Bounds, List<double> Rates) : IRetroPayScoreModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(RetroPayScore item, double valueValue)
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

/// <summary>Runs the retro pay score steps in order and keeps a trace of each.</summary>
public sealed class RetroPayScoreEngine(List<IRetroPayScoreModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IRetroPayScoreModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static RetroPayScoreEngine DefaultRetroPayScoreEngine()
    {
        return new RetroPayScoreEngine(
            new List<IRetroPayScoreModifier>
            {
                new RetroPayScoreCapacityOffsetAdjuster(0.54),
                new MinimumRetroPayScoreAdjuster(31.5),
                new TieredRetroPayScoreAdjuster(
                    new List<double> { 1719.0, 4538.0, 8318.0, 8408.0 },
                    new List<double> { 1.17, 0.8, 1.02, 0.89 }),
            },
            new List<string>(),
            1.5,
            19280.7);
    }

    public double Compute(RetroPayScore item)
    {
        var current = item.ProjectedValue();
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

    public Dictionary<string, double> EvaluateAll(List<RetroPayScore> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
