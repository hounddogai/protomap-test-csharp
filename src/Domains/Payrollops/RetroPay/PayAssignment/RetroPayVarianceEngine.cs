namespace Acme.Workforce.Payrollops.RetroPay.PayAssignment;

/// <summary>One step of the retro pay variance calculation.</summary>
public interface IRetroPayVarianceStep
{
    string Describe();
    double Adjust(RetroPayVariance item, double valueValue);
}

public sealed record MinimumRetroPayVarianceAdjuster(double Minimum) : IRetroPayVarianceStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(RetroPayVariance item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record PercentRetroPayVarianceAdjuster(double Percent) : IRetroPayVarianceStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(RetroPayVariance item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RetroPayVarianceTenureMonthsOffsetAdjuster(double Weight) : IRetroPayVarianceStep
{
    public string Describe()
    {
        return $"plus tenure months times {Weight}";
    }

    public double Adjust(RetroPayVariance item, double valueValue)
    {
        return valueValue + (double)item.TenureMonths * Weight;
    }
}

/// <summary>Runs the retro pay variance steps in order and keeps a trace of each.</summary>
public sealed class RetroPayVarianceEngine(List<IRetroPayVarianceStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IRetroPayVarianceStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static RetroPayVarianceEngine DefaultRetroPayVarianceEngine()
    {
        return new RetroPayVarianceEngine(
            new List<IRetroPayVarianceStep>
            {
                new MinimumRetroPayVarianceAdjuster(48.2),
                new PercentRetroPayVarianceAdjuster(3.6),
                new RetroPayVarianceTenureMonthsOffsetAdjuster(1.46),
            },
            new List<string>(),
            0.3,
            91132.1);
    }

    public double Price(RetroPayVariance item)
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

    public Dictionary<string, double> EvaluateAll(List<RetroPayVariance> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
