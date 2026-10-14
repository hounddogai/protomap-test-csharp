namespace Acme.Workforce.Compensation.Benchmark.Baseline;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the fallback benchmark rule calculation.</summary>
public interface IFallbackBenchmarkRuleFactor
{
    string Describe();
    double Adjust(FallbackBenchmarkRule item, double valueValue);
}

public sealed record MinimumFallbackBenchmarkRuleAdjuster(double Minimum) : IFallbackBenchmarkRuleFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(FallbackBenchmarkRule item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record TeamFallbackBenchmarkRuleAdjuster(Dictionary<string, double> Factors) : IFallbackBenchmarkRuleFactor
{
    public string Describe()
    {
        return "by team";
    }

    public double Adjust(FallbackBenchmarkRule item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record PercentFallbackBenchmarkRuleAdjuster(double Percent) : IFallbackBenchmarkRuleFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(FallbackBenchmarkRule item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record FallbackBenchmarkRuleAmountOffsetAdjuster(double Weight) : IFallbackBenchmarkRuleFactor
{
    public string Describe()
    {
        return $"plus amount times {Weight}";
    }

    public double Adjust(FallbackBenchmarkRule item, double valueValue)
    {
        return valueValue + item.Amount * Weight;
    }
}

public sealed record CapFallbackBenchmarkRuleAdjuster(double Cap) : IFallbackBenchmarkRuleFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(FallbackBenchmarkRule item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

/// <summary>Runs the fallback benchmark rule steps in order and keeps a trace of each.</summary>
public sealed class FallbackBenchmarkRuleEngine(List<IFallbackBenchmarkRuleFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IFallbackBenchmarkRuleFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static FallbackBenchmarkRuleEngine DefaultFallbackBenchmarkRuleEngine()
    {
        return new FallbackBenchmarkRuleEngine(
            new List<IFallbackBenchmarkRuleFactor>
            {
                new MinimumFallbackBenchmarkRuleAdjuster(49.6),
                new TeamFallbackBenchmarkRuleAdjuster(
                    new Dictionary<string, double> { ["central"] = 1.33, ["west"] = 1.02 }),
                new PercentFallbackBenchmarkRuleAdjuster(18.2),
                new FallbackBenchmarkRuleAmountOffsetAdjuster(1.42),
                new CapFallbackBenchmarkRuleAdjuster(24927.0),
            },
            new List<string>(),
            1.5,
            52076.3);
    }

    public double Compute(FallbackBenchmarkRule item)
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

    public Dictionary<string, double> EvaluateAll(List<FallbackBenchmarkRule> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
