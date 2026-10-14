namespace Acme.Workforce.Compensation.MeritCycle.Budget;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the capped merit cycle budget calculation.</summary>
public interface ICappedMeritCycleBudgetFactor
{
    string Describe();
    double Adjust(CappedMeritCycleBudget item, double valueValue);
}

public sealed record FlaggedCappedMeritCycleBudgetAdjuster(Registry Flags, string Flag, double Percent) : ICappedMeritCycleBudgetFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(CappedMeritCycleBudget item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapCappedMeritCycleBudgetAdjuster(double Cap) : ICappedMeritCycleBudgetFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(CappedMeritCycleBudget item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record MinimumCappedMeritCycleBudgetAdjuster(double Minimum) : ICappedMeritCycleBudgetFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(CappedMeritCycleBudget item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record TieredCappedMeritCycleBudgetAdjuster(List<double> Bounds, List<double> Rates) : ICappedMeritCycleBudgetFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(CappedMeritCycleBudget item, double valueValue)
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

public sealed record RoundingCappedMeritCycleBudgetAdjuster(double Step) : ICappedMeritCycleBudgetFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(CappedMeritCycleBudget item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the capped merit cycle budget steps in order and keeps a trace of each.</summary>
public sealed class CappedMeritCycleBudgetEngine(List<ICappedMeritCycleBudgetFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<ICappedMeritCycleBudgetFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static CappedMeritCycleBudgetEngine DefaultCappedMeritCycleBudgetEngine(Registry flags)
    {
        return new CappedMeritCycleBudgetEngine(
            new List<ICappedMeritCycleBudgetFactor>
            {
                new FlaggedCappedMeritCycleBudgetAdjuster(flags, "capped-merit-cycle-budget-batched", -9.9),
                new CapCappedMeritCycleBudgetAdjuster(34020.0),
                new MinimumCappedMeritCycleBudgetAdjuster(2.8),
                new TieredCappedMeritCycleBudgetAdjuster(
                    new List<double> { 638.0, 4414.0, 7331.0, 7456.0 },
                    new List<double> { 0.85, 0.93, 1.08, 1.0 }),
                new RoundingCappedMeritCycleBudgetAdjuster(0.1),
            },
            new List<string>(),
            3.3,
            77959.0);
    }

    public double Evaluate(CappedMeritCycleBudget item)
    {
        var current = item.ProjectedExposure();
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

    public Dictionary<string, double> EvaluateAll(List<CappedMeritCycleBudget> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
