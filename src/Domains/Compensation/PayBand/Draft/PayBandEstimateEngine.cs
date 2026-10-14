namespace Acme.Workforce.Compensation.PayBand.Draft;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the pay band estimate calculation.</summary>
public interface IPayBandEstimateAdjuster
{
    string Describe();
    double Adjust(PayBandEstimate item, double valueValue);
}

public sealed record CapPayBandEstimateAdjuster(double Cap) : IPayBandEstimateAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(PayBandEstimate item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record MinimumPayBandEstimateAdjuster(double Minimum) : IPayBandEstimateAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(PayBandEstimate item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record PercentPayBandEstimateAdjuster(double Percent) : IPayBandEstimateAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(PayBandEstimate item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record PayBandEstimateSequenceOffsetAdjuster(double Weight) : IPayBandEstimateAdjuster
{
    public string Describe()
    {
        return $"plus sequence times {Weight}";
    }

    public double Adjust(PayBandEstimate item, double valueValue)
    {
        return valueValue + (double)item.Sequence * Weight;
    }
}

/// <summary>Runs the pay band estimate steps in order and keeps a trace of each.</summary>
public sealed class PayBandEstimateEngine(List<IPayBandEstimateAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IPayBandEstimateAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static PayBandEstimateEngine DefaultPayBandEstimateEngine()
    {
        return new PayBandEstimateEngine(
            new List<IPayBandEstimateAdjuster>
            {
                new CapPayBandEstimateAdjuster(9344.0),
                new MinimumPayBandEstimateAdjuster(13.4),
                new PercentPayBandEstimateAdjuster(-11.8),
                new PayBandEstimateSequenceOffsetAdjuster(0.45),
            },
            new List<string>(),
            0.8,
            4128.5);
    }

    public double Compute(PayBandEstimate item)
    {
        var current = item.WeightedLoad();
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

    public Dictionary<string, double> EvaluateAll(List<PayBandEstimate> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
