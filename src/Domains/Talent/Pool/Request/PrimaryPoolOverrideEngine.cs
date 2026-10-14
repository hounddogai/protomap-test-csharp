namespace Acme.Workforce.Talent.Pool.Request;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the primary pool override calculation.</summary>
public interface IPrimaryPoolOverrideAdjuster
{
    string Describe();
    double Adjust(PrimaryPoolOverride item, double valueValue);
}

public sealed record CapPrimaryPoolOverrideAdjuster(double Cap) : IPrimaryPoolOverrideAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(PrimaryPoolOverride item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record RoundingPrimaryPoolOverrideAdjuster(double Step) : IPrimaryPoolOverrideAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(PrimaryPoolOverride item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record PercentPrimaryPoolOverrideAdjuster(double Percent) : IPrimaryPoolOverrideAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(PrimaryPoolOverride item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record NamePrimaryPoolOverrideAdjuster(Dictionary<string, double> Factors) : IPrimaryPoolOverrideAdjuster
{
    public string Describe()
    {
        return "by name";
    }

    public double Adjust(PrimaryPoolOverride item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record MinimumPrimaryPoolOverrideAdjuster(double Minimum) : IPrimaryPoolOverrideAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(PrimaryPoolOverride item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the primary pool override steps in order and keeps a trace of each.</summary>
public sealed class PrimaryPoolOverrideEngine(List<IPrimaryPoolOverrideAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IPrimaryPoolOverrideAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static PrimaryPoolOverrideEngine DefaultPrimaryPoolOverrideEngine()
    {
        return new PrimaryPoolOverrideEngine(
            new List<IPrimaryPoolOverrideAdjuster>
            {
                new CapPrimaryPoolOverrideAdjuster(42983.0),
                new RoundingPrimaryPoolOverrideAdjuster(0.1),
                new PercentPrimaryPoolOverrideAdjuster(-9.1),
                new NamePrimaryPoolOverrideAdjuster(
                    new Dictionary<string, double> { ["central"] = 0.87, ["online"] = 1.19 }),
                new MinimumPrimaryPoolOverrideAdjuster(5.9),
            },
            new List<string>(),
            0.3,
            54936.8);
    }

    public double Price(PrimaryPoolOverride item)
    {
        var current = item.ProjectedExposure();
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

    public Dictionary<string, double> EvaluateAll(List<PrimaryPoolOverride> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
