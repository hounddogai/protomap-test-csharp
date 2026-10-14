namespace Acme.Workforce.Payrollops.Deduction.Proposal;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the deduction request calculation.</summary>
public interface IDeductionRequestModifier
{
    string Describe();
    double Adjust(DeductionRequest item, double valueValue);
}

public sealed record MinimumDeductionRequestAdjuster(double Minimum) : IDeductionRequestModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(DeductionRequest item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record TieredDeductionRequestAdjuster(List<double> Bounds, List<double> Rates) : IDeductionRequestModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(DeductionRequest item, double valueValue)
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

public sealed record FlaggedDeductionRequestAdjuster(Registry Flags, string Flag, double Percent) : IDeductionRequestModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(DeductionRequest item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CountryDeductionRequestAdjuster(Dictionary<string, double> Factors) : IDeductionRequestModifier
{
    public string Describe()
    {
        return "by country";
    }

    public double Adjust(DeductionRequest item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record PercentDeductionRequestAdjuster(double Percent) : IDeductionRequestModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(DeductionRequest item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the deduction request steps in order and keeps a trace of each.</summary>
public sealed class DeductionRequestEngine(List<IDeductionRequestModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IDeductionRequestModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static DeductionRequestEngine DefaultDeductionRequestEngine(Registry flags)
    {
        return new DeductionRequestEngine(
            new List<IDeductionRequestModifier>
            {
                new MinimumDeductionRequestAdjuster(40.4),
                new TieredDeductionRequestAdjuster(
                    new List<double> { 3478.0, 4222.0 },
                    new List<double> { 1.07, 1.18 }),
                new FlaggedDeductionRequestAdjuster(flags, "deduction-request-global", 1.4),
                new CountryDeductionRequestAdjuster(
                    new Dictionary<string, double> { ["central"] = 0.74, ["north"] = 1.01, ["online"] = 0.88 }),
                new PercentDeductionRequestAdjuster(-4.8),
            },
            new List<string>(),
            3.8,
            4831.3);
    }

    public double Price(DeductionRequest item)
    {
        var current = item.WeightedLoad();
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

    public Dictionary<string, double> EvaluateAll(List<DeductionRequest> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
