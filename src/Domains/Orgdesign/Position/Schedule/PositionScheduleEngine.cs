namespace Acme.Workforce.Orgdesign.Position.Schedule;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the position schedule calculation.</summary>
public interface IPositionScheduleAdjuster
{
    string Describe();
    double Adjust(PositionSchedule item, double valueValue);
}

public sealed record PositionScheduleTenureMonthsOffsetAdjuster(double Weight) : IPositionScheduleAdjuster
{
    public string Describe()
    {
        return $"plus tenure months times {Weight}";
    }

    public double Adjust(PositionSchedule item, double valueValue)
    {
        return valueValue + (double)item.TenureMonths * Weight;
    }
}

public sealed record PercentPositionScheduleAdjuster(double Percent) : IPositionScheduleAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(PositionSchedule item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record FlaggedPositionScheduleAdjuster(Registry Flags, string Flag, double Percent) : IPositionScheduleAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(PositionSchedule item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapPositionScheduleAdjuster(double Cap) : IPositionScheduleAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(PositionSchedule item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record DepartmentPositionScheduleAdjuster(Dictionary<string, double> Factors) : IPositionScheduleAdjuster
{
    public string Describe()
    {
        return "by department";
    }

    public double Adjust(PositionSchedule item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record MinimumPositionScheduleAdjuster(double Minimum) : IPositionScheduleAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(PositionSchedule item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the position schedule steps in order and keeps a trace of each.</summary>
public sealed class PositionScheduleEngine(List<IPositionScheduleAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IPositionScheduleAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static PositionScheduleEngine DefaultPositionScheduleEngine(Registry flags)
    {
        return new PositionScheduleEngine(
            new List<IPositionScheduleAdjuster>
            {
                new PositionScheduleTenureMonthsOffsetAdjuster(1.33),
                new PercentPositionScheduleAdjuster(5.6),
                new FlaggedPositionScheduleAdjuster(flags, "position-schedule-weighted", 3.4),
                new CapPositionScheduleAdjuster(30083.0),
                new DepartmentPositionScheduleAdjuster(
                    new Dictionary<string, double> { ["south"] = 0.76, ["retail"] = 1.09, ["east"] = 0.77 }),
                new MinimumPositionScheduleAdjuster(2.1),
            },
            new List<string>(),
            0.0,
            25273.6);
    }

    public double Compute(PositionSchedule item)
    {
        var current = item.EffectiveVolume();
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

    public Dictionary<string, double> EvaluateAll(List<PositionSchedule> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
