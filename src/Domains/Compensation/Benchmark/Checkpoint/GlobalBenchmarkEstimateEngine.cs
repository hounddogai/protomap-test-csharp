namespace Acme.Workforce.Compensation.Benchmark.Checkpoint;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the global benchmark estimate calculation.</summary>
public interface IGlobalBenchmarkEstimateStep
{
    string Describe();
    double Adjust(GlobalBenchmarkEstimate item, double valueValue);
}

public sealed record MinimumGlobalBenchmarkEstimateAdjuster(double Minimum) : IGlobalBenchmarkEstimateStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(GlobalBenchmarkEstimate item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record NameGlobalBenchmarkEstimateAdjuster(Dictionary<string, double> Factors) : IGlobalBenchmarkEstimateStep
{
    public string Describe()
    {
        return "by name";
    }

    public double Adjust(GlobalBenchmarkEstimate item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record RoundingGlobalBenchmarkEstimateAdjuster(double Step) : IGlobalBenchmarkEstimateStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(GlobalBenchmarkEstimate item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record GlobalBenchmarkEstimateQuantityOffsetAdjuster(double Weight) : IGlobalBenchmarkEstimateStep
{
    public string Describe()
    {
        return $"plus quantity times {Weight}";
    }

    public double Adjust(GlobalBenchmarkEstimate item, double valueValue)
    {
        return valueValue + (double)item.Quantity * Weight;
    }
}

/// <summary>Runs the global benchmark estimate steps in order and keeps a trace of each.</summary>
public sealed class GlobalBenchmarkEstimateEngine(List<IGlobalBenchmarkEstimateStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IGlobalBenchmarkEstimateStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static GlobalBenchmarkEstimateEngine DefaultGlobalBenchmarkEstimateEngine()
    {
        return new GlobalBenchmarkEstimateEngine(
            new List<IGlobalBenchmarkEstimateStep>
            {
                new MinimumGlobalBenchmarkEstimateAdjuster(0.8),
                new NameGlobalBenchmarkEstimateAdjuster(
                    new Dictionary<string, double> { ["online"] = 1.35, ["south"] = 1.16 }),
                new RoundingGlobalBenchmarkEstimateAdjuster(0.05),
                new GlobalBenchmarkEstimateQuantityOffsetAdjuster(1.74),
            },
            new List<string>(),
            0.8,
            68640.6);
    }

    public double Price(GlobalBenchmarkEstimate item)
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

    public Dictionary<string, double> EvaluateAll(List<GlobalBenchmarkEstimate> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
