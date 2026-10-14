namespace Acme.Workforce.Payrollops.PayRun.Bundle;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the pay run summary calculation.</summary>
public interface IPayRunSummaryFactor
{
    string Describe();
    double Adjust(PayRunSummary item, double valueValue);
}

public sealed record CapPayRunSummaryAdjuster(double Cap) : IPayRunSummaryFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(PayRunSummary item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record MinimumPayRunSummaryAdjuster(double Minimum) : IPayRunSummaryFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(PayRunSummary item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record RoundingPayRunSummaryAdjuster(double Step) : IPayRunSummaryFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(PayRunSummary item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record PayRunSummarySequenceOffsetAdjuster(double Weight) : IPayRunSummaryFactor
{
    public string Describe()
    {
        return $"plus sequence times {Weight}";
    }

    public double Adjust(PayRunSummary item, double valueValue)
    {
        return valueValue + (double)item.Sequence * Weight;
    }
}

/// <summary>Runs the pay run summary steps in order and keeps a trace of each.</summary>
public sealed class PayRunSummaryEngine(List<IPayRunSummaryFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IPayRunSummaryFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static PayRunSummaryEngine DefaultPayRunSummaryEngine()
    {
        return new PayRunSummaryEngine(
            new List<IPayRunSummaryFactor>
            {
                new CapPayRunSummaryAdjuster(31787.0),
                new MinimumPayRunSummaryAdjuster(24.5),
                new RoundingPayRunSummaryAdjuster(0.1),
                new PayRunSummarySequenceOffsetAdjuster(1.27),
            },
            new List<string>(),
            0.6,
            39190.6);
    }

    public double Evaluate(PayRunSummary item)
    {
        var current = item.AdjustedExposure();
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

    public Dictionary<string, double> EvaluateAll(List<PayRunSummary> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
