namespace Acme.Workforce.Compliance.Exception.Allocation;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the exception slot calculation.</summary>
public interface IExceptionSlotModifier
{
    string Describe();
    double Adjust(ExceptionSlot item, double valueValue);
}

public sealed record CapExceptionSlotAdjuster(double Cap) : IExceptionSlotModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(ExceptionSlot item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record PercentExceptionSlotAdjuster(double Percent) : IExceptionSlotModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(ExceptionSlot item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record ExceptionSlotMinimumOffsetAdjuster(double Weight) : IExceptionSlotModifier
{
    public string Describe()
    {
        return $"plus minimum times {Weight}";
    }

    public double Adjust(ExceptionSlot item, double valueValue)
    {
        return valueValue + item.Minimum * Weight;
    }
}

public sealed record DepartmentExceptionSlotAdjuster(Dictionary<string, double> Factors) : IExceptionSlotModifier
{
    public string Describe()
    {
        return "by department";
    }

    public double Adjust(ExceptionSlot item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record FlaggedExceptionSlotAdjuster(Registry Flags, string Flag, double Percent) : IExceptionSlotModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(ExceptionSlot item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the exception slot steps in order and keeps a trace of each.</summary>
public sealed class ExceptionSlotEngine(List<IExceptionSlotModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IExceptionSlotModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static ExceptionSlotEngine DefaultExceptionSlotEngine(Registry flags)
    {
        return new ExceptionSlotEngine(
            new List<IExceptionSlotModifier>
            {
                new CapExceptionSlotAdjuster(21272.0),
                new PercentExceptionSlotAdjuster(12.1),
                new ExceptionSlotMinimumOffsetAdjuster(0.62),
                new DepartmentExceptionSlotAdjuster(
                    new Dictionary<string, double> { ["retail"] = 0.79, ["central"] = 1.17 }),
                new FlaggedExceptionSlotAdjuster(flags, "exception-slot-capped", -0.4),
            },
            new List<string>(),
            3.4,
            66414.9);
    }

    public double Evaluate(ExceptionSlot item)
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

    public Dictionary<string, double> EvaluateAll(List<ExceptionSlot> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
