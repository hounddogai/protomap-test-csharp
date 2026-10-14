namespace Acme.Workforce.Payrollops.PayRun.CappedRunLedger;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the default pay run ledger calculation.</summary>
public interface IDefaultPayRunLedgerModifier
{
    string Describe();
    double Adjust(DefaultPayRunLedger item, double valueValue);
}

public sealed record MinimumDefaultPayRunLedgerAdjuster(double Minimum) : IDefaultPayRunLedgerModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(DefaultPayRunLedger item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record DefaultPayRunLedgerGraceDaysOffsetAdjuster(double Weight) : IDefaultPayRunLedgerModifier
{
    public string Describe()
    {
        return $"plus grace days times {Weight}";
    }

    public double Adjust(DefaultPayRunLedger item, double valueValue)
    {
        return valueValue + (double)item.GraceDays * Weight;
    }
}

public sealed record FlaggedDefaultPayRunLedgerAdjuster(Registry Flags, string Flag, double Percent) : IDefaultPayRunLedgerModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(DefaultPayRunLedger item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record SourceDefaultPayRunLedgerAdjuster(Dictionary<string, double> Factors) : IDefaultPayRunLedgerModifier
{
    public string Describe()
    {
        return "by source";
    }

    public double Adjust(DefaultPayRunLedger item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record CapDefaultPayRunLedgerAdjuster(double Cap) : IDefaultPayRunLedgerModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(DefaultPayRunLedger item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record RoundingDefaultPayRunLedgerAdjuster(double Step) : IDefaultPayRunLedgerModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(DefaultPayRunLedger item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the default pay run ledger steps in order and keeps a trace of each.</summary>
public sealed class DefaultPayRunLedgerEngine(List<IDefaultPayRunLedgerModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IDefaultPayRunLedgerModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static DefaultPayRunLedgerEngine DefaultDefaultPayRunLedgerEngine(Registry flags)
    {
        return new DefaultPayRunLedgerEngine(
            new List<IDefaultPayRunLedgerModifier>
            {
                new MinimumDefaultPayRunLedgerAdjuster(40.6),
                new DefaultPayRunLedgerGraceDaysOffsetAdjuster(0.27),
                new FlaggedDefaultPayRunLedgerAdjuster(flags, "default-pay-run-ledger-strict", -7.9),
                new SourceDefaultPayRunLedgerAdjuster(
                    new Dictionary<string, double> { ["south"] = 0.85, ["north"] = 0.8 }),
                new CapDefaultPayRunLedgerAdjuster(5621.0),
                new RoundingDefaultPayRunLedgerAdjuster(0.25),
            },
            new List<string>(),
            3.1,
            75216.6);
    }

    public double Compute(DefaultPayRunLedger item)
    {
        var current = item.NetLoad();
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

    public Dictionary<string, double> EvaluateAll(List<DefaultPayRunLedger> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
