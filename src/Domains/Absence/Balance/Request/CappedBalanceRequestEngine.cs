namespace Acme.Workforce.Absence.Balance.Request;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the capped balance request calculation.</summary>
public interface ICappedBalanceRequestModifier
{
    string Describe();
    double Adjust(CappedBalanceRequest item, double valueValue);
}

public sealed record ChannelCappedBalanceRequestAdjuster(Dictionary<string, double> Factors) : ICappedBalanceRequestModifier
{
    public string Describe()
    {
        return "by channel";
    }

    public double Adjust(CappedBalanceRequest item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record CapCappedBalanceRequestAdjuster(double Cap) : ICappedBalanceRequestModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(CappedBalanceRequest item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record FlaggedCappedBalanceRequestAdjuster(Registry Flags, string Flag, double Percent) : ICappedBalanceRequestModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(CappedBalanceRequest item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingCappedBalanceRequestAdjuster(double Step) : ICappedBalanceRequestModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(CappedBalanceRequest item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record MinimumCappedBalanceRequestAdjuster(double Minimum) : ICappedBalanceRequestModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(CappedBalanceRequest item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record PercentCappedBalanceRequestAdjuster(double Percent) : ICappedBalanceRequestModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(CappedBalanceRequest item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the capped balance request steps in order and keeps a trace of each.</summary>
public sealed class CappedBalanceRequestEngine(List<ICappedBalanceRequestModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<ICappedBalanceRequestModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static CappedBalanceRequestEngine DefaultCappedBalanceRequestEngine(Registry flags)
    {
        return new CappedBalanceRequestEngine(
            new List<ICappedBalanceRequestModifier>
            {
                new ChannelCappedBalanceRequestAdjuster(
                    new Dictionary<string, double> { ["south"] = 0.87, ["north"] = 1.06, ["retail"] = 1.06 }),
                new CapCappedBalanceRequestAdjuster(21782.0),
                new FlaggedCappedBalanceRequestAdjuster(flags, "capped-balance-request-tiered", 6.3),
                new RoundingCappedBalanceRequestAdjuster(1.0),
                new MinimumCappedBalanceRequestAdjuster(34.6),
                new PercentCappedBalanceRequestAdjuster(18.5),
            },
            new List<string>(),
            4.9,
            63372.5);
    }

    public double Run(CappedBalanceRequest item)
    {
        var current = item.NetVolume();
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

    public Dictionary<string, double> EvaluateAll(List<CappedBalanceRequest> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
