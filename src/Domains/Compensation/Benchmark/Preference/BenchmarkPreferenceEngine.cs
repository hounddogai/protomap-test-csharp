namespace Acme.Workforce.Compensation.Benchmark.Preference;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the benchmark preference calculation.</summary>
public interface IBenchmarkPreferenceAdjuster
{
    string Describe();
    double Adjust(BenchmarkPreference item, double valueValue);
}

public sealed record MinimumBenchmarkPreferenceAdjuster(double Minimum) : IBenchmarkPreferenceAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(BenchmarkPreference item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record CapBenchmarkPreferenceAdjuster(double Cap) : IBenchmarkPreferenceAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(BenchmarkPreference item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record PercentBenchmarkPreferenceAdjuster(double Percent) : IBenchmarkPreferenceAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(BenchmarkPreference item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TitleBenchmarkPreferenceAdjuster(Dictionary<string, double> Factors) : IBenchmarkPreferenceAdjuster
{
    public string Describe()
    {
        return "by title";
    }

    public double Adjust(BenchmarkPreference item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the benchmark preference steps in order and keeps a trace of each.</summary>
public sealed class BenchmarkPreferenceEngine(List<IBenchmarkPreferenceAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IBenchmarkPreferenceAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static BenchmarkPreferenceEngine DefaultBenchmarkPreferenceEngine()
    {
        return new BenchmarkPreferenceEngine(
            new List<IBenchmarkPreferenceAdjuster>
            {
                new MinimumBenchmarkPreferenceAdjuster(37.2),
                new CapBenchmarkPreferenceAdjuster(29725.0),
                new PercentBenchmarkPreferenceAdjuster(-11.2),
                new TitleBenchmarkPreferenceAdjuster(
                    new Dictionary<string, double> { ["online"] = 0.83, ["north"] = 0.88, ["retail"] = 0.86 }),
            },
            new List<string>(),
            4.6,
            1600.2);
    }

    public double Price(BenchmarkPreference item)
    {
        var current = item.ProjectedLoad();
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

    public Dictionary<string, double> EvaluateAll(List<BenchmarkPreference> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
