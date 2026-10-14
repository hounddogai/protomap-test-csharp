namespace Acme.Workforce.Payrollops.RetroPay.Bundle;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the retro pay ledger calculation.</summary>
public interface IRetroPayLedgerModifier
{
    string Describe();
    double Adjust(RetroPayLedger item, double valueValue);
}

public sealed record MinimumRetroPayLedgerAdjuster(double Minimum) : IRetroPayLedgerModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(RetroPayLedger item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record RoundingRetroPayLedgerAdjuster(double Step) : IRetroPayLedgerModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(RetroPayLedger item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record CapRetroPayLedgerAdjuster(double Cap) : IRetroPayLedgerModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(RetroPayLedger item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record PercentRetroPayLedgerAdjuster(double Percent) : IRetroPayLedgerModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(RetroPayLedger item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TitleRetroPayLedgerAdjuster(Dictionary<string, double> Factors) : IRetroPayLedgerModifier
{
    public string Describe()
    {
        return "by title";
    }

    public double Adjust(RetroPayLedger item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the retro pay ledger steps in order and keeps a trace of each.</summary>
public sealed class RetroPayLedgerEngine(List<IRetroPayLedgerModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IRetroPayLedgerModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static RetroPayLedgerEngine DefaultRetroPayLedgerEngine()
    {
        return new RetroPayLedgerEngine(
            new List<IRetroPayLedgerModifier>
            {
                new MinimumRetroPayLedgerAdjuster(3.9),
                new RoundingRetroPayLedgerAdjuster(5.0),
                new CapRetroPayLedgerAdjuster(2997.0),
                new PercentRetroPayLedgerAdjuster(-21.2),
                new TitleRetroPayLedgerAdjuster(new Dictionary<string, double> { ["south"] = 1.11 }),
            },
            new List<string>(),
            1.0,
            20416.5);
    }

    public double Evaluate(RetroPayLedger item)
    {
        var current = item.EffectiveLoad();
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

    public Dictionary<string, double> EvaluateAll(List<RetroPayLedger> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
