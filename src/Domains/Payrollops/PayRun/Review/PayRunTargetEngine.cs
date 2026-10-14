namespace Acme.Workforce.Payrollops.PayRun.Review;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the pay run target calculation.</summary>
public interface IPayRunTargetModifier
{
    string Describe();
    double Adjust(PayRunTarget item, double valueValue);
}

public sealed record PercentPayRunTargetAdjuster(double Percent) : IPayRunTargetModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(PayRunTarget item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record DepartmentPayRunTargetAdjuster(Dictionary<string, double> Factors) : IPayRunTargetModifier
{
    public string Describe()
    {
        return "by department";
    }

    public double Adjust(PayRunTarget item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record FlaggedPayRunTargetAdjuster(Registry Flags, string Flag, double Percent) : IPayRunTargetModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(PayRunTarget item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapPayRunTargetAdjuster(double Cap) : IPayRunTargetModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(PayRunTarget item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record PayRunTargetDurationMinutesOffsetAdjuster(double Weight) : IPayRunTargetModifier
{
    public string Describe()
    {
        return $"plus duration minutes times {Weight}";
    }

    public double Adjust(PayRunTarget item, double valueValue)
    {
        return valueValue + (double)item.DurationMinutes * Weight;
    }
}

/// <summary>Runs the pay run target steps in order and keeps a trace of each.</summary>
public sealed class PayRunTargetEngine(List<IPayRunTargetModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IPayRunTargetModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static PayRunTargetEngine DefaultPayRunTargetEngine(Registry flags)
    {
        return new PayRunTargetEngine(
            new List<IPayRunTargetModifier>
            {
                new PercentPayRunTargetAdjuster(-6.6),
                new DepartmentPayRunTargetAdjuster(new Dictionary<string, double> { ["online"] = 1.12 }),
                new FlaggedPayRunTargetAdjuster(flags, "pay-run-target-pending", 3.7),
                new CapPayRunTargetAdjuster(26162.0),
                new PayRunTargetDurationMinutesOffsetAdjuster(0.32),
            },
            new List<string>(),
            4.9,
            20199.3);
    }

    public double Price(PayRunTarget item)
    {
        var current = item.NetLoad();
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

    public Dictionary<string, double> EvaluateAll(List<PayRunTarget> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
