namespace Acme.Workforce.Compensation.EquityGrant.Agreement;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the pending equity grant agreement calculation.</summary>
public interface IPendingEquityGrantAgreementModifier
{
    string Describe();
    double Adjust(PendingEquityGrantAgreement item, double valueValue);
}

public sealed record FlaggedPendingEquityGrantAgreementAdjuster(Registry Flags, string Flag, double Percent) : IPendingEquityGrantAgreementModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(PendingEquityGrantAgreement item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record PendingEquityGrantAgreementMaximumOffsetAdjuster(double Weight) : IPendingEquityGrantAgreementModifier
{
    public string Describe()
    {
        return $"plus maximum times {Weight}";
    }

    public double Adjust(PendingEquityGrantAgreement item, double valueValue)
    {
        return valueValue + item.Maximum * Weight;
    }
}

public sealed record LocalePendingEquityGrantAgreementAdjuster(Dictionary<string, double> Factors) : IPendingEquityGrantAgreementModifier
{
    public string Describe()
    {
        return "by locale";
    }

    public double Adjust(PendingEquityGrantAgreement item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record MinimumPendingEquityGrantAgreementAdjuster(double Minimum) : IPendingEquityGrantAgreementModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(PendingEquityGrantAgreement item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record PercentPendingEquityGrantAgreementAdjuster(double Percent) : IPendingEquityGrantAgreementModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(PendingEquityGrantAgreement item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the pending equity grant agreement steps in order and keeps a trace of each.</summary>
public sealed class PendingEquityGrantAgreementEngine(List<IPendingEquityGrantAgreementModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IPendingEquityGrantAgreementModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static PendingEquityGrantAgreementEngine DefaultPendingEquityGrantAgreementEngine(Registry flags)
    {
        return new PendingEquityGrantAgreementEngine(
            new List<IPendingEquityGrantAgreementModifier>
            {
                new FlaggedPendingEquityGrantAgreementAdjuster(flags, "pending-equity-grant-agreement-global", 7.2),
                new PendingEquityGrantAgreementMaximumOffsetAdjuster(0.36),
                new LocalePendingEquityGrantAgreementAdjuster(new Dictionary<string, double> { ["west"] = 0.98 }),
                new MinimumPendingEquityGrantAgreementAdjuster(29.6),
                new PercentPendingEquityGrantAgreementAdjuster(-23.9),
            },
            new List<string>(),
            1.6,
            55703.5);
    }

    public double Price(PendingEquityGrantAgreement item)
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

    public Dictionary<string, double> EvaluateAll(List<PendingEquityGrantAgreement> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
