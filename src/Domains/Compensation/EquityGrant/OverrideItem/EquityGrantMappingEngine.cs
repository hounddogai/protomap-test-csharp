namespace Acme.Workforce.Compensation.EquityGrant.OverrideItem;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the equity grant mapping calculation.</summary>
public interface IEquityGrantMappingFactor
{
    string Describe();
    double Adjust(EquityGrantMapping item, double valueValue);
}

public sealed record EquityGrantMappingBudgetOffsetAdjuster(double Weight) : IEquityGrantMappingFactor
{
    public string Describe()
    {
        return $"plus budget times {Weight}";
    }

    public double Adjust(EquityGrantMapping item, double valueValue)
    {
        return valueValue + item.Budget * Weight;
    }
}

public sealed record FlaggedEquityGrantMappingAdjuster(Registry Flags, string Flag, double Percent) : IEquityGrantMappingFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(EquityGrantMapping item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumEquityGrantMappingAdjuster(double Minimum) : IEquityGrantMappingFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(EquityGrantMapping item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record RoundingEquityGrantMappingAdjuster(double Step) : IEquityGrantMappingFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(EquityGrantMapping item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record PercentEquityGrantMappingAdjuster(double Percent) : IEquityGrantMappingFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(EquityGrantMapping item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapEquityGrantMappingAdjuster(double Cap) : IEquityGrantMappingFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(EquityGrantMapping item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

/// <summary>Runs the equity grant mapping steps in order and keeps a trace of each.</summary>
public sealed class EquityGrantMappingEngine(List<IEquityGrantMappingFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IEquityGrantMappingFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static EquityGrantMappingEngine DefaultEquityGrantMappingEngine(Registry flags)
    {
        return new EquityGrantMappingEngine(
            new List<IEquityGrantMappingFactor>
            {
                new EquityGrantMappingBudgetOffsetAdjuster(0.43),
                new FlaggedEquityGrantMappingAdjuster(flags, "equity-grant-mapping-fallback", -0.6),
                new MinimumEquityGrantMappingAdjuster(8.8),
                new RoundingEquityGrantMappingAdjuster(0.25),
                new PercentEquityGrantMappingAdjuster(-17.3),
                new CapEquityGrantMappingAdjuster(16468.0),
            },
            new List<string>(),
            4.5,
            40783.7);
    }

    public double Run(EquityGrantMapping item)
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

    public Dictionary<string, double> EvaluateAll(List<EquityGrantMapping> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
