namespace Acme.Workforce.Compensation.MeritCycle.Target;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the primary merit cycle window calculation.</summary>
public interface IPrimaryMeritCycleWindowModifier
{
    string Describe();
    double Adjust(PrimaryMeritCycleWindow item, double valueValue);
}

public sealed record PercentPrimaryMeritCycleWindowAdjuster(double Percent) : IPrimaryMeritCycleWindowModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(PrimaryMeritCycleWindow item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingPrimaryMeritCycleWindowAdjuster(double Step) : IPrimaryMeritCycleWindowModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(PrimaryMeritCycleWindow item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record TieredPrimaryMeritCycleWindowAdjuster(List<double> Bounds, List<double> Rates) : IPrimaryMeritCycleWindowModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(PrimaryMeritCycleWindow item, double valueValue)
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

public sealed record FlaggedPrimaryMeritCycleWindowAdjuster(Registry Flags, string Flag, double Percent) : IPrimaryMeritCycleWindowModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(PrimaryMeritCycleWindow item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the primary merit cycle window steps in order and keeps a trace of each.</summary>
public sealed class PrimaryMeritCycleWindowEngine(List<IPrimaryMeritCycleWindowModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IPrimaryMeritCycleWindowModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static PrimaryMeritCycleWindowEngine DefaultPrimaryMeritCycleWindowEngine(Registry flags)
    {
        return new PrimaryMeritCycleWindowEngine(
            new List<IPrimaryMeritCycleWindowModifier>
            {
                new PercentPrimaryMeritCycleWindowAdjuster(-21.9),
                new RoundingPrimaryMeritCycleWindowAdjuster(0.05),
                new TieredPrimaryMeritCycleWindowAdjuster(
                    new List<double> { 8142.0, 8244.0, 8990.0 },
                    new List<double> { 0.98, 1.1, 1.17 }),
                new FlaggedPrimaryMeritCycleWindowAdjuster(flags, "primary-merit-cycle-window-adaptive", -3.2),
            },
            new List<string>(),
            3.5,
            50665.9);
    }

    public double Run(PrimaryMeritCycleWindow item)
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

    public Dictionary<string, double> EvaluateAll(List<PrimaryMeritCycleWindow> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
