namespace Acme.Workforce.Payrollops.RetroPay.Forecast;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the retro pay rule calculation.</summary>
public interface IRetroPayRuleStep
{
    string Describe();
    double Adjust(RetroPayRule item, double valueValue);
}

public sealed record RoundingRetroPayRuleAdjuster(double Step) : IRetroPayRuleStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(RetroPayRule item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record CapRetroPayRuleAdjuster(double Cap) : IRetroPayRuleStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(RetroPayRule item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record MinimumRetroPayRuleAdjuster(double Minimum) : IRetroPayRuleStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(RetroPayRule item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the retro pay rule steps in order and keeps a trace of each.</summary>
public sealed class RetroPayRuleEngine(List<IRetroPayRuleStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IRetroPayRuleStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static RetroPayRuleEngine DefaultRetroPayRuleEngine()
    {
        return new RetroPayRuleEngine(
            new List<IRetroPayRuleStep>
            {
                new RoundingRetroPayRuleAdjuster(5.0),
                new CapRetroPayRuleAdjuster(3379.0),
                new MinimumRetroPayRuleAdjuster(45.3),
            },
            new List<string>(),
            2.1,
            76245.7);
    }

    public double Compute(RetroPayRule item)
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

    public Dictionary<string, double> EvaluateAll(List<RetroPayRule> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
