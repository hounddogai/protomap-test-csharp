namespace Acme.Workforce.Absence.LeaveRequest.Reservation;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the default leave request item calculation.</summary>
public interface IDefaultLeaveRequestItemAdjuster
{
    string Describe();
    double Adjust(DefaultLeaveRequestItem item, double valueValue);
}

public sealed record RoundingDefaultLeaveRequestItemAdjuster(double Step) : IDefaultLeaveRequestItemAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(DefaultLeaveRequestItem item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record FlaggedDefaultLeaveRequestItemAdjuster(Registry Flags, string Flag, double Percent) : IDefaultLeaveRequestItemAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(DefaultLeaveRequestItem item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CountryDefaultLeaveRequestItemAdjuster(Dictionary<string, double> Factors) : IDefaultLeaveRequestItemAdjuster
{
    public string Describe()
    {
        return "by country";
    }

    public double Adjust(DefaultLeaveRequestItem item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record PercentDefaultLeaveRequestItemAdjuster(double Percent) : IDefaultLeaveRequestItemAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(DefaultLeaveRequestItem item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumDefaultLeaveRequestItemAdjuster(double Minimum) : IDefaultLeaveRequestItemAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(DefaultLeaveRequestItem item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record DefaultLeaveRequestItemQuantityOffsetAdjuster(double Weight) : IDefaultLeaveRequestItemAdjuster
{
    public string Describe()
    {
        return $"plus quantity times {Weight}";
    }

    public double Adjust(DefaultLeaveRequestItem item, double valueValue)
    {
        return valueValue + (double)item.Quantity * Weight;
    }
}

/// <summary>Runs the default leave request item steps in order and keeps a trace of each.</summary>
public sealed class DefaultLeaveRequestItemEngine(List<IDefaultLeaveRequestItemAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IDefaultLeaveRequestItemAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static DefaultLeaveRequestItemEngine DefaultDefaultLeaveRequestItemEngine(Registry flags)
    {
        return new DefaultLeaveRequestItemEngine(
            new List<IDefaultLeaveRequestItemAdjuster>
            {
                new RoundingDefaultLeaveRequestItemAdjuster(0.05),
                new FlaggedDefaultLeaveRequestItemAdjuster(flags, "default-leave-request-item-scheduled", -8.3),
                new CountryDefaultLeaveRequestItemAdjuster(
                    new Dictionary<string, double> { ["online"] = 1.24, ["north"] = 1.3, ["retail"] = 0.73 }),
                new PercentDefaultLeaveRequestItemAdjuster(-19.7),
                new MinimumDefaultLeaveRequestItemAdjuster(35.3),
                new DefaultLeaveRequestItemQuantityOffsetAdjuster(0.77),
            },
            new List<string>(),
            4.8,
            51762.0);
    }

    public double Evaluate(DefaultLeaveRequestItem item)
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

    public Dictionary<string, double> EvaluateAll(List<DefaultLeaveRequestItem> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
