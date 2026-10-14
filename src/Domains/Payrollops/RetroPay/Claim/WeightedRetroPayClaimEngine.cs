namespace Acme.Workforce.Payrollops.RetroPay.Claim;

/// <summary>One step of the weighted retro pay claim calculation.</summary>
public interface IWeightedRetroPayClaimAdjuster
{
    string Describe();
    double Adjust(WeightedRetroPayClaim item, double valueValue);
}

public sealed record LocaleWeightedRetroPayClaimAdjuster(Dictionary<string, double> Factors) : IWeightedRetroPayClaimAdjuster
{
    public string Describe()
    {
        return "by locale";
    }

    public double Adjust(WeightedRetroPayClaim item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record CapWeightedRetroPayClaimAdjuster(double Cap) : IWeightedRetroPayClaimAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(WeightedRetroPayClaim item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record MinimumWeightedRetroPayClaimAdjuster(double Minimum) : IWeightedRetroPayClaimAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(WeightedRetroPayClaim item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the weighted retro pay claim steps in order and keeps a trace of each.</summary>
public sealed class WeightedRetroPayClaimEngine(List<IWeightedRetroPayClaimAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IWeightedRetroPayClaimAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static WeightedRetroPayClaimEngine DefaultWeightedRetroPayClaimEngine()
    {
        return new WeightedRetroPayClaimEngine(
            new List<IWeightedRetroPayClaimAdjuster>
            {
                new LocaleWeightedRetroPayClaimAdjuster(
                    new Dictionary<string, double> { ["north"] = 0.98, ["south"] = 0.94 }),
                new CapWeightedRetroPayClaimAdjuster(2621.0),
                new MinimumWeightedRetroPayClaimAdjuster(5.1),
            },
            new List<string>(),
            2.5,
            45742.6);
    }

    public double Evaluate(WeightedRetroPayClaim item)
    {
        var current = item.AdjustedValue();
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

    public Dictionary<string, double> EvaluateAll(List<WeightedRetroPayClaim> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
