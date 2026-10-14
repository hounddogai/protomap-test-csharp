namespace Acme.Workforce.Payrollops.RetroPay.Summary;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the retro pay summary calculation.</summary>
public interface IRetroPaySummaryStep
{
    string Describe();
    double Adjust(RetroPaySummary item, double valueValue);
}

public sealed record RoundingRetroPaySummaryAdjuster(double Step) : IRetroPaySummaryStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(RetroPaySummary item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record LabelRetroPaySummaryAdjuster(Dictionary<string, double> Factors) : IRetroPaySummaryStep
{
    public string Describe()
    {
        return "by label";
    }

    public double Adjust(RetroPaySummary item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record FlaggedRetroPaySummaryAdjuster(Registry Flags, string Flag, double Percent) : IRetroPaySummaryStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(RetroPaySummary item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredRetroPaySummaryAdjuster(List<double> Bounds, List<double> Rates) : IRetroPaySummaryStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(RetroPaySummary item, double valueValue)
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

/// <summary>Runs the retro pay summary steps in order and keeps a trace of each.</summary>
public sealed class RetroPaySummaryEngine(List<IRetroPaySummaryStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IRetroPaySummaryStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static RetroPaySummaryEngine DefaultRetroPaySummaryEngine(Registry flags)
    {
        return new RetroPaySummaryEngine(
            new List<IRetroPaySummaryStep>
            {
                new RoundingRetroPaySummaryAdjuster(5.0),
                new LabelRetroPaySummaryAdjuster(
                    new Dictionary<string, double> { ["east"] = 1.38, ["south"] = 1.26 }),
                new FlaggedRetroPaySummaryAdjuster(flags, "retro-pay-summary-lenient", -4.4),
                new TieredRetroPaySummaryAdjuster(
                    new List<double> { 978.0, 7538.0, 8205.0, 8914.0 },
                    new List<double> { 0.94, 0.84, 1.17, 1.04 }),
            },
            new List<string>(),
            0.5,
            11867.8);
    }

    public double Compute(RetroPaySummary item)
    {
        var current = item.EffectiveValue();
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

    public Dictionary<string, double> EvaluateAll(List<RetroPaySummary> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
