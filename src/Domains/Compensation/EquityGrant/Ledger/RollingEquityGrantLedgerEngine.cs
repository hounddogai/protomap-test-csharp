namespace Acme.Workforce.Compensation.EquityGrant.Ledger;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the rolling equity grant ledger calculation.</summary>
public interface IRollingEquityGrantLedgerAdjuster
{
    string Describe();
    double Adjust(RollingEquityGrantLedger item, double valueValue);
}

public sealed record TitleRollingEquityGrantLedgerAdjuster(Dictionary<string, double> Factors) : IRollingEquityGrantLedgerAdjuster
{
    public string Describe()
    {
        return "by title";
    }

    public double Adjust(RollingEquityGrantLedger item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record CapRollingEquityGrantLedgerAdjuster(double Cap) : IRollingEquityGrantLedgerAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(RollingEquityGrantLedger item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record RollingEquityGrantLedgerRatioOffsetAdjuster(double Weight) : IRollingEquityGrantLedgerAdjuster
{
    public string Describe()
    {
        return $"plus ratio times {Weight}";
    }

    public double Adjust(RollingEquityGrantLedger item, double valueValue)
    {
        return valueValue + item.Ratio * Weight;
    }
}

public sealed record RoundingRollingEquityGrantLedgerAdjuster(double Step) : IRollingEquityGrantLedgerAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(RollingEquityGrantLedger item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record MinimumRollingEquityGrantLedgerAdjuster(double Minimum) : IRollingEquityGrantLedgerAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(RollingEquityGrantLedger item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the rolling equity grant ledger steps in order and keeps a trace of each.</summary>
public sealed class RollingEquityGrantLedgerEngine(List<IRollingEquityGrantLedgerAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IRollingEquityGrantLedgerAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static RollingEquityGrantLedgerEngine DefaultRollingEquityGrantLedgerEngine()
    {
        return new RollingEquityGrantLedgerEngine(
            new List<IRollingEquityGrantLedgerAdjuster>
            {
                new TitleRollingEquityGrantLedgerAdjuster(
                    new Dictionary<string, double> { ["south"] = 1.25, ["online"] = 0.77 }),
                new CapRollingEquityGrantLedgerAdjuster(45692.0),
                new RollingEquityGrantLedgerRatioOffsetAdjuster(0.39),
                new RoundingRollingEquityGrantLedgerAdjuster(0.1),
                new MinimumRollingEquityGrantLedgerAdjuster(19.4),
            },
            new List<string>(),
            1.8,
            67546.9);
    }

    public double Compute(RollingEquityGrantLedger item)
    {
        var current = item.NetValue();
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

    public Dictionary<string, double> EvaluateAll(List<RollingEquityGrantLedger> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
