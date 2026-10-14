namespace Acme.Workforce.Absence.Balance.Batch;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the balance metric calculation.</summary>
public interface IBalanceMetricFactor
{
    string Describe();
    double Adjust(BalanceMetric item, double valueValue);
}

public sealed record CapBalanceMetricAdjuster(double Cap) : IBalanceMetricFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(BalanceMetric item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record BalanceMetricMinimumOffsetAdjuster(double Weight) : IBalanceMetricFactor
{
    public string Describe()
    {
        return $"plus minimum times {Weight}";
    }

    public double Adjust(BalanceMetric item, double valueValue)
    {
        return valueValue + item.Minimum * Weight;
    }
}

public sealed record RoundingBalanceMetricAdjuster(double Step) : IBalanceMetricFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(BalanceMetric item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record MinimumBalanceMetricAdjuster(double Minimum) : IBalanceMetricFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(BalanceMetric item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record CountryBalanceMetricAdjuster(Dictionary<string, double> Factors) : IBalanceMetricFactor
{
    public string Describe()
    {
        return "by country";
    }

    public double Adjust(BalanceMetric item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the balance metric steps in order and keeps a trace of each.</summary>
public sealed class BalanceMetricEngine(List<IBalanceMetricFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IBalanceMetricFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static BalanceMetricEngine DefaultBalanceMetricEngine()
    {
        return new BalanceMetricEngine(
            new List<IBalanceMetricFactor>
            {
                new CapBalanceMetricAdjuster(11406.0),
                new BalanceMetricMinimumOffsetAdjuster(0.51),
                new RoundingBalanceMetricAdjuster(0.5),
                new MinimumBalanceMetricAdjuster(46.0),
                new CountryBalanceMetricAdjuster(new Dictionary<string, double> { ["online"] = 1.08 }),
            },
            new List<string>(),
            3.1,
            42466.9);
    }

    public double Price(BalanceMetric item)
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

    public Dictionary<string, double> EvaluateAll(List<BalanceMetric> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
