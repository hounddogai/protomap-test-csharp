namespace Acme.Workforce.Payrollops.Deduction.Batch;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the deduction profile calculation.</summary>
public interface IDeductionProfileFactor
{
    string Describe();
    double Adjust(DeductionProfile item, double valueValue);
}

public sealed record PercentDeductionProfileAdjuster(double Percent) : IDeductionProfileFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(DeductionProfile item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapDeductionProfileAdjuster(double Cap) : IDeductionProfileFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(DeductionProfile item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record DeductionProfileCostOffsetAdjuster(double Weight) : IDeductionProfileFactor
{
    public string Describe()
    {
        return $"plus cost times {Weight}";
    }

    public double Adjust(DeductionProfile item, double valueValue)
    {
        return valueValue + item.Cost * Weight;
    }
}

public sealed record RoundingDeductionProfileAdjuster(double Step) : IDeductionProfileFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(DeductionProfile item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the deduction profile steps in order and keeps a trace of each.</summary>
public sealed class DeductionProfileEngine(List<IDeductionProfileFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IDeductionProfileFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static DeductionProfileEngine DefaultDeductionProfileEngine()
    {
        return new DeductionProfileEngine(
            new List<IDeductionProfileFactor>
            {
                new PercentDeductionProfileAdjuster(0.4),
                new CapDeductionProfileAdjuster(15290.0),
                new DeductionProfileCostOffsetAdjuster(1.15),
                new RoundingDeductionProfileAdjuster(1.0),
            },
            new List<string>(),
            0.5,
            85435.4);
    }

    public double Evaluate(DeductionProfile item)
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

    public Dictionary<string, double> EvaluateAll(List<DeductionProfile> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
