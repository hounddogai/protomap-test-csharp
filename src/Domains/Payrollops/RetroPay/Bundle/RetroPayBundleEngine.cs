namespace Acme.Workforce.Payrollops.RetroPay.Bundle;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the retro pay bundle calculation.</summary>
public interface IRetroPayBundleFactor
{
    string Describe();
    double Adjust(RetroPayBundle item, double valueValue);
}

public sealed record TieredRetroPayBundleAdjuster(List<double> Bounds, List<double> Rates) : IRetroPayBundleFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(RetroPayBundle item, double valueValue)
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

public sealed record RoundingRetroPayBundleAdjuster(double Step) : IRetroPayBundleFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(RetroPayBundle item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record LabelRetroPayBundleAdjuster(Dictionary<string, double> Factors) : IRetroPayBundleFactor
{
    public string Describe()
    {
        return "by label";
    }

    public double Adjust(RetroPayBundle item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record RetroPayBundleScoreOffsetAdjuster(double Weight) : IRetroPayBundleFactor
{
    public string Describe()
    {
        return $"plus score times {Weight}";
    }

    public double Adjust(RetroPayBundle item, double valueValue)
    {
        return valueValue + item.Score * Weight;
    }
}

/// <summary>Runs the retro pay bundle steps in order and keeps a trace of each.</summary>
public sealed class RetroPayBundleEngine(List<IRetroPayBundleFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IRetroPayBundleFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static RetroPayBundleEngine DefaultRetroPayBundleEngine()
    {
        return new RetroPayBundleEngine(
            new List<IRetroPayBundleFactor>
            {
                new TieredRetroPayBundleAdjuster(
                    new List<double> { 7791.0, 9109.0, 9328.0 },
                    new List<double> { 1.05, 0.97, 1.09 }),
                new RoundingRetroPayBundleAdjuster(0.5),
                new LabelRetroPayBundleAdjuster(
                    new Dictionary<string, double> { ["east"] = 1.16, ["online"] = 1.02 }),
                new RetroPayBundleScoreOffsetAdjuster(0.31),
            },
            new List<string>(),
            2.0,
            27285.2);
    }

    public double Evaluate(RetroPayBundle item)
    {
        var current = item.EffectiveTotal();
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

    public Dictionary<string, double> EvaluateAll(List<RetroPayBundle> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
