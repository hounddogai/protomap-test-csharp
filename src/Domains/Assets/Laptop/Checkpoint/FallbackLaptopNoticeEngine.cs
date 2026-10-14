namespace Acme.Workforce.Assets.Laptop.Checkpoint;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the fallback laptop notice calculation.</summary>
public interface IFallbackLaptopNoticeAdjuster
{
    string Describe();
    double Adjust(FallbackLaptopNotice item, double valueValue);
}

public sealed record CapFallbackLaptopNoticeAdjuster(double Cap) : IFallbackLaptopNoticeAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(FallbackLaptopNotice item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record FlaggedFallbackLaptopNoticeAdjuster(Registry Flags, string Flag, double Percent) : IFallbackLaptopNoticeAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(FallbackLaptopNotice item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record DepartmentFallbackLaptopNoticeAdjuster(Dictionary<string, double> Factors) : IFallbackLaptopNoticeAdjuster
{
    public string Describe()
    {
        return "by department";
    }

    public double Adjust(FallbackLaptopNotice item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record RoundingFallbackLaptopNoticeAdjuster(double Step) : IFallbackLaptopNoticeAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(FallbackLaptopNotice item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record PercentFallbackLaptopNoticeAdjuster(double Percent) : IFallbackLaptopNoticeAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(FallbackLaptopNotice item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record FallbackLaptopNoticeBudgetOffsetAdjuster(double Weight) : IFallbackLaptopNoticeAdjuster
{
    public string Describe()
    {
        return $"plus budget times {Weight}";
    }

    public double Adjust(FallbackLaptopNotice item, double valueValue)
    {
        return valueValue + item.Budget * Weight;
    }
}

/// <summary>Runs the fallback laptop notice steps in order and keeps a trace of each.</summary>
public sealed class FallbackLaptopNoticeEngine(List<IFallbackLaptopNoticeAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IFallbackLaptopNoticeAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static FallbackLaptopNoticeEngine DefaultFallbackLaptopNoticeEngine(Registry flags)
    {
        return new FallbackLaptopNoticeEngine(
            new List<IFallbackLaptopNoticeAdjuster>
            {
                new CapFallbackLaptopNoticeAdjuster(49418.0),
                new FlaggedFallbackLaptopNoticeAdjuster(flags, "fallback-laptop-notice-capped", -9.8),
                new DepartmentFallbackLaptopNoticeAdjuster(new Dictionary<string, double> { ["south"] = 1.0 }),
                new RoundingFallbackLaptopNoticeAdjuster(0.25),
                new PercentFallbackLaptopNoticeAdjuster(-1.2),
                new FallbackLaptopNoticeBudgetOffsetAdjuster(0.77),
            },
            new List<string>(),
            2.5,
            61746.4);
    }

    public double Compute(FallbackLaptopNotice item)
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

    public Dictionary<string, double> EvaluateAll(List<FallbackLaptopNotice> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
