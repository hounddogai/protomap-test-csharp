namespace Acme.Workforce.Payrollops.RetroPay.Tier;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the capped retro pay preference calculation.</summary>
public interface ICappedRetroPayPreferenceAdjuster
{
    string Describe();
    double Adjust(CappedRetroPayPreference item, double valueValue);
}

public sealed record MinimumCappedRetroPayPreferenceAdjuster(double Minimum) : ICappedRetroPayPreferenceAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(CappedRetroPayPreference item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record CountryCappedRetroPayPreferenceAdjuster(Dictionary<string, double> Factors) : ICappedRetroPayPreferenceAdjuster
{
    public string Describe()
    {
        return "by country";
    }

    public double Adjust(CappedRetroPayPreference item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record PercentCappedRetroPayPreferenceAdjuster(double Percent) : ICappedRetroPayPreferenceAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(CappedRetroPayPreference item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the capped retro pay preference steps in order and keeps a trace of each.</summary>
public sealed class CappedRetroPayPreferenceEngine(List<ICappedRetroPayPreferenceAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<ICappedRetroPayPreferenceAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static CappedRetroPayPreferenceEngine DefaultCappedRetroPayPreferenceEngine()
    {
        return new CappedRetroPayPreferenceEngine(
            new List<ICappedRetroPayPreferenceAdjuster>
            {
                new MinimumCappedRetroPayPreferenceAdjuster(10.1),
                new CountryCappedRetroPayPreferenceAdjuster(
                    new Dictionary<string, double> { ["west"] = 1.3, ["central"] = 0.78 }),
                new PercentCappedRetroPayPreferenceAdjuster(-5.7),
            },
            new List<string>(),
            5.0,
            77221.0);
    }

    public double Price(CappedRetroPayPreference item)
    {
        var current = item.MeasuredValue();
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

    public Dictionary<string, double> EvaluateAll(List<CappedRetroPayPreference> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
