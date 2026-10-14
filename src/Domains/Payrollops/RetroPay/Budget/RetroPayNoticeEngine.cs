namespace Acme.Workforce.Payrollops.RetroPay.Budget;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the retro pay notice calculation.</summary>
public interface IRetroPayNoticeModifier
{
    string Describe();
    double Adjust(RetroPayNotice item, double valueValue);
}

public sealed record RoundingRetroPayNoticeAdjuster(double Step) : IRetroPayNoticeModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(RetroPayNotice item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record FlaggedRetroPayNoticeAdjuster(Registry Flags, string Flag, double Percent) : IRetroPayNoticeModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(RetroPayNotice item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record PercentRetroPayNoticeAdjuster(double Percent) : IRetroPayNoticeModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(RetroPayNotice item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumRetroPayNoticeAdjuster(double Minimum) : IRetroPayNoticeModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(RetroPayNotice item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record TieredRetroPayNoticeAdjuster(List<double> Bounds, List<double> Rates) : IRetroPayNoticeModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(RetroPayNotice item, double valueValue)
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

public sealed record LabelRetroPayNoticeAdjuster(Dictionary<string, double> Factors) : IRetroPayNoticeModifier
{
    public string Describe()
    {
        return "by label";
    }

    public double Adjust(RetroPayNotice item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the retro pay notice steps in order and keeps a trace of each.</summary>
public sealed class RetroPayNoticeEngine(List<IRetroPayNoticeModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IRetroPayNoticeModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static RetroPayNoticeEngine DefaultRetroPayNoticeEngine(Registry flags)
    {
        return new RetroPayNoticeEngine(
            new List<IRetroPayNoticeModifier>
            {
                new RoundingRetroPayNoticeAdjuster(0.05),
                new FlaggedRetroPayNoticeAdjuster(flags, "retro-pay-notice-batched", -7.5),
                new PercentRetroPayNoticeAdjuster(20.0),
                new MinimumRetroPayNoticeAdjuster(31.4),
                new TieredRetroPayNoticeAdjuster(
                    new List<double> { 6323.0, 8083.0, 8387.0 },
                    new List<double> { 0.86, 1.1, 1.08 }),
                new LabelRetroPayNoticeAdjuster(
                    new Dictionary<string, double> { ["retail"] = 1.12, ["south"] = 0.81, ["west"] = 0.98 }),
            },
            new List<string>(),
            4.4,
            7685.8);
    }

    public double Price(RetroPayNotice item)
    {
        var current = item.ProjectedValue();
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

    public Dictionary<string, double> EvaluateAll(List<RetroPayNotice> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
