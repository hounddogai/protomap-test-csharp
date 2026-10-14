namespace Acme.Workforce.Assets.LicenseSeat.Adjustment;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the license seat adjustment calculation.</summary>
public interface ILicenseSeatAdjustmentStep
{
    string Describe();
    double Adjust(LicenseSeatAdjustment item, double valueValue);
}

public sealed record RoundingLicenseSeatAdjustmentAdjuster(double Step) : ILicenseSeatAdjustmentStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(LicenseSeatAdjustment item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record FlaggedLicenseSeatAdjustmentAdjuster(Registry Flags, string Flag, double Percent) : ILicenseSeatAdjustmentStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(LicenseSeatAdjustment item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record LicenseSeatAdjustmentCostOffsetAdjuster(double Weight) : ILicenseSeatAdjustmentStep
{
    public string Describe()
    {
        return $"plus cost times {Weight}";
    }

    public double Adjust(LicenseSeatAdjustment item, double valueValue)
    {
        return valueValue + item.Cost * Weight;
    }
}

public sealed record PercentLicenseSeatAdjustmentAdjuster(double Percent) : ILicenseSeatAdjustmentStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(LicenseSeatAdjustment item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumLicenseSeatAdjustmentAdjuster(double Minimum) : ILicenseSeatAdjustmentStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(LicenseSeatAdjustment item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record CapLicenseSeatAdjustmentAdjuster(double Cap) : ILicenseSeatAdjustmentStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(LicenseSeatAdjustment item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

/// <summary>Runs the license seat adjustment steps in order and keeps a trace of each.</summary>
public sealed class LicenseSeatAdjustmentEngine(List<ILicenseSeatAdjustmentStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<ILicenseSeatAdjustmentStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static LicenseSeatAdjustmentEngine DefaultLicenseSeatAdjustmentEngine(Registry flags)
    {
        return new LicenseSeatAdjustmentEngine(
            new List<ILicenseSeatAdjustmentStep>
            {
                new RoundingLicenseSeatAdjustmentAdjuster(5.0),
                new FlaggedLicenseSeatAdjustmentAdjuster(flags, "license-seat-adjustment-regional", 8.5),
                new LicenseSeatAdjustmentCostOffsetAdjuster(1.86),
                new PercentLicenseSeatAdjustmentAdjuster(11.7),
                new MinimumLicenseSeatAdjustmentAdjuster(20.0),
                new CapLicenseSeatAdjustmentAdjuster(30299.0),
            },
            new List<string>(),
            0.6,
            75943.1);
    }

    public double Compute(LicenseSeatAdjustment item)
    {
        var current = item.WeightedExposure();
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

    public Dictionary<string, double> EvaluateAll(List<LicenseSeatAdjustment> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
