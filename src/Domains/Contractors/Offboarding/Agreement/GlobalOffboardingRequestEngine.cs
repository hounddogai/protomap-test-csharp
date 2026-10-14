namespace Acme.Workforce.Contractors.Offboarding.Agreement;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the global offboarding request calculation.</summary>
public interface IGlobalOffboardingRequestAdjuster
{
    string Describe();
    double Adjust(GlobalOffboardingRequest item, double valueValue);
}

public sealed record PercentGlobalOffboardingRequestAdjuster(double Percent) : IGlobalOffboardingRequestAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(GlobalOffboardingRequest item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredGlobalOffboardingRequestAdjuster(List<double> Bounds, List<double> Rates) : IGlobalOffboardingRequestAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(GlobalOffboardingRequest item, double valueValue)
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

public sealed record CapGlobalOffboardingRequestAdjuster(double Cap) : IGlobalOffboardingRequestAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(GlobalOffboardingRequest item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record RoundingGlobalOffboardingRequestAdjuster(double Step) : IGlobalOffboardingRequestAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(GlobalOffboardingRequest item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record MinimumGlobalOffboardingRequestAdjuster(double Minimum) : IGlobalOffboardingRequestAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(GlobalOffboardingRequest item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the global offboarding request steps in order and keeps a trace of each.</summary>
public sealed class GlobalOffboardingRequestEngine(List<IGlobalOffboardingRequestAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IGlobalOffboardingRequestAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static GlobalOffboardingRequestEngine DefaultGlobalOffboardingRequestEngine()
    {
        return new GlobalOffboardingRequestEngine(
            new List<IGlobalOffboardingRequestAdjuster>
            {
                new PercentGlobalOffboardingRequestAdjuster(-18.5),
                new TieredGlobalOffboardingRequestAdjuster(
                    new List<double> { 283.0, 5776.0, 6706.0 },
                    new List<double> { 1.01, 0.81, 1.08 }),
                new CapGlobalOffboardingRequestAdjuster(19643.0),
                new RoundingGlobalOffboardingRequestAdjuster(1.0),
                new MinimumGlobalOffboardingRequestAdjuster(22.6),
            },
            new List<string>(),
            3.2,
            2842.6);
    }

    public double Evaluate(GlobalOffboardingRequest item)
    {
        var current = item.MeasuredValue();
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

    public Dictionary<string, double> EvaluateAll(List<GlobalOffboardingRequest> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
