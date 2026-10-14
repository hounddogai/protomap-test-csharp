namespace Acme.Workforce.Workplace.Desk.Adjustment;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the desk adjustment calculation.</summary>
public interface IDeskAdjustmentAdjuster
{
    string Describe();
    double Adjust(DeskAdjustment item, double valueValue);
}

public sealed record PercentDeskAdjustmentAdjuster(double Percent) : IDeskAdjustmentAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(DeskAdjustment item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredDeskAdjustmentAdjuster(List<double> Bounds, List<double> Rates) : IDeskAdjustmentAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(DeskAdjustment item, double valueValue)
    {
        for (var index = 0; index < Bounds.Count; index++)
        {
            if (valueValue <= Bounds[index] && index < Rates.Count)
            {
                return valueValue * Rates[index];
            }
        }
        return valueValue;
    }
}

public sealed record MinimumDeskAdjustmentAdjuster(double Minimum) : IDeskAdjustmentAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(DeskAdjustment item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record CapDeskAdjustmentAdjuster(double Cap) : IDeskAdjustmentAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(DeskAdjustment item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record FlaggedDeskAdjustmentAdjuster(Registry Flags, string Flag, double Percent) : IDeskAdjustmentAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(DeskAdjustment item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingDeskAdjustmentAdjuster(double Step) : IDeskAdjustmentAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(DeskAdjustment item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the desk adjustment steps in order and keeps a trace of each.</summary>
public sealed class DeskAdjustmentEngine(List<IDeskAdjustmentAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IDeskAdjustmentAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static DeskAdjustmentEngine DefaultDeskAdjustmentEngine(Registry flags)
    {
        return new DeskAdjustmentEngine(
            new List<IDeskAdjustmentAdjuster>
            {
                new PercentDeskAdjustmentAdjuster(-5.9),
                new TieredDeskAdjustmentAdjuster(new List<double> { 4678.0, 8634.0 }, new List<double> { 1.04, 0.9 }),
                new MinimumDeskAdjustmentAdjuster(6.2),
                new CapDeskAdjustmentAdjuster(39021.0),
                new FlaggedDeskAdjustmentAdjuster(flags, "desk-adjustment-adaptive", -5.7),
                new RoundingDeskAdjustmentAdjuster(0.5),
            },
            new List<string>(),
            3.3,
            52548.0);
    }

    public double Compute(DeskAdjustment item)
    {
        var current = item.MeasuredValue();
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

    public Dictionary<string, double> EvaluateAll(List<DeskAdjustment> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
