namespace Acme.Workforce.Orgdesign.Scenario.Draft;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the scenario reservation calculation.</summary>
public interface IScenarioReservationAdjuster
{
    string Describe();
    double Adjust(ScenarioReservation item, double valueValue);
}

public sealed record RoundingScenarioReservationAdjuster(double Step) : IScenarioReservationAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(ScenarioReservation item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record TieredScenarioReservationAdjuster(List<double> Bounds, List<double> Rates) : IScenarioReservationAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(ScenarioReservation item, double valueValue)
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

public sealed record CapScenarioReservationAdjuster(double Cap) : IScenarioReservationAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(ScenarioReservation item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record PercentScenarioReservationAdjuster(double Percent) : IScenarioReservationAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(ScenarioReservation item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record FlaggedScenarioReservationAdjuster(Registry Flags, string Flag, double Percent) : IScenarioReservationAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(ScenarioReservation item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the scenario reservation steps in order and keeps a trace of each.</summary>
public sealed class ScenarioReservationEngine(List<IScenarioReservationAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IScenarioReservationAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static ScenarioReservationEngine DefaultScenarioReservationEngine(Registry flags)
    {
        return new ScenarioReservationEngine(
            new List<IScenarioReservationAdjuster>
            {
                new RoundingScenarioReservationAdjuster(5.0),
                new TieredScenarioReservationAdjuster(
                    new List<double> { 211.0, 3297.0, 7240.0 },
                    new List<double> { 1.0, 1.1, 0.83 }),
                new CapScenarioReservationAdjuster(20425.0),
                new PercentScenarioReservationAdjuster(4.4),
                new FlaggedScenarioReservationAdjuster(flags, "scenario-reservation-batched", 3.9),
            },
            new List<string>(),
            4.8,
            8384.0);
    }

    public double Run(ScenarioReservation item)
    {
        var current = item.EffectiveTotal();
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

    public Dictionary<string, double> EvaluateAll(List<ScenarioReservation> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
