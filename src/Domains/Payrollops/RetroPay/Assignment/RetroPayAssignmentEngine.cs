namespace Acme.Workforce.Payrollops.RetroPay.Assignment;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the retro pay assignment calculation.</summary>
public interface IRetroPayAssignmentFactor
{
    string Describe();
    double Adjust(RetroPayAssignment item, double valueValue);
}

public sealed record FlaggedRetroPayAssignmentAdjuster(Registry Flags, string Flag, double Percent) : IRetroPayAssignmentFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(RetroPayAssignment item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record PercentRetroPayAssignmentAdjuster(double Percent) : IRetroPayAssignmentFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(RetroPayAssignment item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumRetroPayAssignmentAdjuster(double Minimum) : IRetroPayAssignmentFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(RetroPayAssignment item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record RoundingRetroPayAssignmentAdjuster(double Step) : IRetroPayAssignmentFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(RetroPayAssignment item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the retro pay assignment steps in order and keeps a trace of each.</summary>
public sealed class RetroPayAssignmentEngine(List<IRetroPayAssignmentFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IRetroPayAssignmentFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static RetroPayAssignmentEngine DefaultRetroPayAssignmentEngine(Registry flags)
    {
        return new RetroPayAssignmentEngine(
            new List<IRetroPayAssignmentFactor>
            {
                new FlaggedRetroPayAssignmentAdjuster(flags, "retro-pay-assignment-pending", 0.1),
                new PercentRetroPayAssignmentAdjuster(-15.6),
                new MinimumRetroPayAssignmentAdjuster(50.0),
                new RoundingRetroPayAssignmentAdjuster(5.0),
            },
            new List<string>(),
            2.2,
            18915.8);
    }

    public double Run(RetroPayAssignment item)
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

    public Dictionary<string, double> EvaluateAll(List<RetroPayAssignment> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
