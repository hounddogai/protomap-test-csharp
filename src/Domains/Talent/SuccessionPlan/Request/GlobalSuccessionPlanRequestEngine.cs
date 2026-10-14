namespace Acme.Workforce.Talent.SuccessionPlan.Request;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the global succession plan request calculation.</summary>
public interface IGlobalSuccessionPlanRequestModifier
{
    string Describe();
    double Adjust(GlobalSuccessionPlanRequest item, double valueValue);
}

public sealed record TieredGlobalSuccessionPlanRequestAdjuster(List<double> Bounds, List<double> Rates) : IGlobalSuccessionPlanRequestModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(GlobalSuccessionPlanRequest item, double valueValue)
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

public sealed record MinimumGlobalSuccessionPlanRequestAdjuster(double Minimum) : IGlobalSuccessionPlanRequestModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(GlobalSuccessionPlanRequest item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record FlaggedGlobalSuccessionPlanRequestAdjuster(Registry Flags, string Flag, double Percent) : IGlobalSuccessionPlanRequestModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(GlobalSuccessionPlanRequest item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapGlobalSuccessionPlanRequestAdjuster(double Cap) : IGlobalSuccessionPlanRequestModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(GlobalSuccessionPlanRequest item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record RoundingGlobalSuccessionPlanRequestAdjuster(double Step) : IGlobalSuccessionPlanRequestModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(GlobalSuccessionPlanRequest item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record PercentGlobalSuccessionPlanRequestAdjuster(double Percent) : IGlobalSuccessionPlanRequestModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(GlobalSuccessionPlanRequest item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the global succession plan request steps in order and keeps a trace of each.</summary>
public sealed class GlobalSuccessionPlanRequestEngine(List<IGlobalSuccessionPlanRequestModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IGlobalSuccessionPlanRequestModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static GlobalSuccessionPlanRequestEngine DefaultGlobalSuccessionPlanRequestEngine(Registry flags)
    {
        return new GlobalSuccessionPlanRequestEngine(
            new List<IGlobalSuccessionPlanRequestModifier>
            {
                new TieredGlobalSuccessionPlanRequestAdjuster(
                    new List<double> { 1628.0, 8580.0 },
                    new List<double> { 0.82, 1.07 }),
                new MinimumGlobalSuccessionPlanRequestAdjuster(36.7),
                new FlaggedGlobalSuccessionPlanRequestAdjuster(flags, "global-succession-plan-request-default", -7.3),
                new CapGlobalSuccessionPlanRequestAdjuster(17871.0),
                new RoundingGlobalSuccessionPlanRequestAdjuster(0.05),
                new PercentGlobalSuccessionPlanRequestAdjuster(-0.7),
            },
            new List<string>(),
            4.1,
            61783.0);
    }

    public double Run(GlobalSuccessionPlanRequest item)
    {
        var current = item.AdjustedValue();
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

    public Dictionary<string, double> EvaluateAll(List<GlobalSuccessionPlanRequest> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
