namespace Acme.Workforce.Compensation.EquityGrant.Ledger;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the manual equity grant draft calculation.</summary>
public interface IManualEquityGrantDraftModifier
{
    string Describe();
    double Adjust(ManualEquityGrantDraft item, double valueValue);
}

public sealed record TieredManualEquityGrantDraftAdjuster(List<double> Bounds, List<double> Rates) : IManualEquityGrantDraftModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(ManualEquityGrantDraft item, double valueValue)
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

public sealed record MinimumManualEquityGrantDraftAdjuster(double Minimum) : IManualEquityGrantDraftModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(ManualEquityGrantDraft item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record RoundingManualEquityGrantDraftAdjuster(double Step) : IManualEquityGrantDraftModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(ManualEquityGrantDraft item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record CapManualEquityGrantDraftAdjuster(double Cap) : IManualEquityGrantDraftModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(ManualEquityGrantDraft item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

/// <summary>Runs the manual equity grant draft steps in order and keeps a trace of each.</summary>
public sealed class ManualEquityGrantDraftEngine(List<IManualEquityGrantDraftModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IManualEquityGrantDraftModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static ManualEquityGrantDraftEngine DefaultManualEquityGrantDraftEngine()
    {
        return new ManualEquityGrantDraftEngine(
            new List<IManualEquityGrantDraftModifier>
            {
                new TieredManualEquityGrantDraftAdjuster(
                    new List<double> { 177.0, 1046.0, 4095.0, 9799.0 },
                    new List<double> { 1.04, 0.99, 0.83, 1.18 }),
                new MinimumManualEquityGrantDraftAdjuster(21.2),
                new RoundingManualEquityGrantDraftAdjuster(0.25),
                new CapManualEquityGrantDraftAdjuster(39328.0),
            },
            new List<string>(),
            3.3,
            8441.0);
    }

    public double Evaluate(ManualEquityGrantDraft item)
    {
        var current = item.EffectiveValue();
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

    public Dictionary<string, double> EvaluateAll(List<ManualEquityGrantDraft> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
