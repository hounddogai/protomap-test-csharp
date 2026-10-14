namespace Acme.Workforce.Orgdesign.Span.Baseline;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the default span profile calculation.</summary>
public interface IDefaultSpanProfileFactor
{
    string Describe();
    double Adjust(DefaultSpanProfile item, double valueValue);
}

public sealed record DepartmentDefaultSpanProfileAdjuster(Dictionary<string, double> Factors) : IDefaultSpanProfileFactor
{
    public string Describe()
    {
        return "by department";
    }

    public double Adjust(DefaultSpanProfile item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record PercentDefaultSpanProfileAdjuster(double Percent) : IDefaultSpanProfileFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(DefaultSpanProfile item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapDefaultSpanProfileAdjuster(double Cap) : IDefaultSpanProfileFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(DefaultSpanProfile item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

/// <summary>Runs the default span profile steps in order and keeps a trace of each.</summary>
public sealed class DefaultSpanProfileEngine(List<IDefaultSpanProfileFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IDefaultSpanProfileFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static DefaultSpanProfileEngine DefaultDefaultSpanProfileEngine()
    {
        return new DefaultSpanProfileEngine(
            new List<IDefaultSpanProfileFactor>
            {
                new DepartmentDefaultSpanProfileAdjuster(
                    new Dictionary<string, double> { ["retail"] = 1.32, ["east"] = 0.7 }),
                new PercentDefaultSpanProfileAdjuster(7.2),
                new CapDefaultSpanProfileAdjuster(33955.0),
            },
            new List<string>(),
            1.0,
            87668.5);
    }

    public double Run(DefaultSpanProfile item)
    {
        var current = item.AdjustedVolume();
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

    public Dictionary<string, double> EvaluateAll(List<DefaultSpanProfile> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
