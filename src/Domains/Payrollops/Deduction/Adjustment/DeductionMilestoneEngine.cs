namespace Acme.Workforce.Payrollops.Deduction.Adjustment;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the deduction milestone calculation.</summary>
public interface IDeductionMilestoneAdjuster
{
    string Describe();
    double Adjust(DeductionMilestone item, double valueValue);
}

public sealed record CapDeductionMilestoneAdjuster(double Cap) : IDeductionMilestoneAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(DeductionMilestone item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record DeductionMilestoneWeightOffsetAdjuster(double Weight) : IDeductionMilestoneAdjuster
{
    public string Describe()
    {
        return $"plus weight times {Weight}";
    }

    public double Adjust(DeductionMilestone item, double valueValue)
    {
        return valueValue + item.Weight * Weight;
    }
}

public sealed record PercentDeductionMilestoneAdjuster(double Percent) : IDeductionMilestoneAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(DeductionMilestone item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingDeductionMilestoneAdjuster(double Step) : IDeductionMilestoneAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(DeductionMilestone item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record MinimumDeductionMilestoneAdjuster(double Minimum) : IDeductionMilestoneAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(DeductionMilestone item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the deduction milestone steps in order and keeps a trace of each.</summary>
public sealed class DeductionMilestoneEngine(List<IDeductionMilestoneAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IDeductionMilestoneAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static DeductionMilestoneEngine DefaultDeductionMilestoneEngine()
    {
        return new DeductionMilestoneEngine(
            new List<IDeductionMilestoneAdjuster>
            {
                new CapDeductionMilestoneAdjuster(39644.0),
                new DeductionMilestoneWeightOffsetAdjuster(1.77),
                new PercentDeductionMilestoneAdjuster(-2.1),
                new RoundingDeductionMilestoneAdjuster(0.5),
                new MinimumDeductionMilestoneAdjuster(23.2),
            },
            new List<string>(),
            3.3,
            38336.8);
    }

    public double Run(DeductionMilestone item)
    {
        var current = item.NetVolume();
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

    public Dictionary<string, double> EvaluateAll(List<DeductionMilestone> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
