namespace Acme.Workforce.Payrollops.RetroPay.Window;

/// <summary>One step of the retro pay estimate calculation.</summary>
public interface IRetroPayEstimateStep
{
    string Describe();
    double Adjust(RetroPayEstimate item, double valueValue);
}

public sealed record CapRetroPayEstimateAdjuster(double Cap) : IRetroPayEstimateStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(RetroPayEstimate item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record RetroPayEstimateScoreOffsetAdjuster(double Weight) : IRetroPayEstimateStep
{
    public string Describe()
    {
        return $"plus score times {Weight}";
    }

    public double Adjust(RetroPayEstimate item, double valueValue)
    {
        return valueValue + item.Score * Weight;
    }
}

public sealed record PercentRetroPayEstimateAdjuster(double Percent) : IRetroPayEstimateStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(RetroPayEstimate item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TitleRetroPayEstimateAdjuster(Dictionary<string, double> Factors) : IRetroPayEstimateStep
{
    public string Describe()
    {
        return "by title";
    }

    public double Adjust(RetroPayEstimate item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the retro pay estimate steps in order and keeps a trace of each.</summary>
public sealed class RetroPayEstimateEngine(List<IRetroPayEstimateStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IRetroPayEstimateStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static RetroPayEstimateEngine DefaultRetroPayEstimateEngine()
    {
        return new RetroPayEstimateEngine(
            new List<IRetroPayEstimateStep>
            {
                new CapRetroPayEstimateAdjuster(49054.0),
                new RetroPayEstimateScoreOffsetAdjuster(0.49),
                new PercentRetroPayEstimateAdjuster(-6.4),
                new TitleRetroPayEstimateAdjuster(
                    new Dictionary<string, double> { ["west"] = 1.18, ["north"] = 1.08, ["retail"] = 1.35 }),
            },
            new List<string>(),
            0.8,
            77341.9);
    }

    public double Run(RetroPayEstimate item)
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

    public Dictionary<string, double> EvaluateAll(List<RetroPayEstimate> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
