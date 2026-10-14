namespace Acme.Workforce.Workplace.Desk.Mapping;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the desk override calculation.</summary>
public interface IDeskOverrideAdjuster
{
    string Describe();
    double Adjust(DeskOverride item, double valueValue);
}

public sealed record RoundingDeskOverrideAdjuster(double Step) : IDeskOverrideAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(DeskOverride item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record FlaggedDeskOverrideAdjuster(Registry Flags, string Flag, double Percent) : IDeskOverrideAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(DeskOverride item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record LabelDeskOverrideAdjuster(Dictionary<string, double> Factors) : IDeskOverrideAdjuster
{
    public string Describe()
    {
        return "by label";
    }

    public double Adjust(DeskOverride item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record TieredDeskOverrideAdjuster(List<double> Bounds, List<double> Rates) : IDeskOverrideAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(DeskOverride item, double valueValue)
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

/// <summary>Runs the desk override steps in order and keeps a trace of each.</summary>
public sealed class DeskOverrideEngine(List<IDeskOverrideAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IDeskOverrideAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static DeskOverrideEngine DefaultDeskOverrideEngine(Registry flags)
    {
        return new DeskOverrideEngine(
            new List<IDeskOverrideAdjuster>
            {
                new RoundingDeskOverrideAdjuster(0.5),
                new FlaggedDeskOverrideAdjuster(flags, "desk-override-scheduled", 6.1),
                new LabelDeskOverrideAdjuster(
                    new Dictionary<string, double> { ["central"] = 0.85, ["south"] = 1.24 }),
                new TieredDeskOverrideAdjuster(
                    new List<double> { 1768.0, 3054.0, 9359.0 },
                    new List<double> { 0.91, 1.1, 0.9 }),
            },
            new List<string>(),
            0.5,
            35181.5);
    }

    public double Price(DeskOverride item)
    {
        var current = item.WeightedValue();
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

    public Dictionary<string, double> EvaluateAll(List<DeskOverride> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
