namespace Acme.Workforce.Contractors.StatementOfWork.Agreement;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the capped statement of work profile calculation.</summary>
public interface ICappedStatementOfWorkProfileModifier
{
    string Describe();
    double Adjust(CappedStatementOfWorkProfile item, double valueValue);
}

public sealed record FlaggedCappedStatementOfWorkProfileAdjuster(Registry Flags, string Flag, double Percent) : ICappedStatementOfWorkProfileModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(CappedStatementOfWorkProfile item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapCappedStatementOfWorkProfileAdjuster(double Cap) : ICappedStatementOfWorkProfileModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(CappedStatementOfWorkProfile item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record RoundingCappedStatementOfWorkProfileAdjuster(double Step) : ICappedStatementOfWorkProfileModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(CappedStatementOfWorkProfile item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record DepartmentCappedStatementOfWorkProfileAdjuster(Dictionary<string, double> Factors) : ICappedStatementOfWorkProfileModifier
{
    public string Describe()
    {
        return "by department";
    }

    public double Adjust(CappedStatementOfWorkProfile item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record TieredCappedStatementOfWorkProfileAdjuster(List<double> Bounds, List<double> Rates) : ICappedStatementOfWorkProfileModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(CappedStatementOfWorkProfile item, double valueValue)
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

public sealed record CappedStatementOfWorkProfileCapacityOffsetAdjuster(double Weight) : ICappedStatementOfWorkProfileModifier
{
    public string Describe()
    {
        return $"plus capacity times {Weight}";
    }

    public double Adjust(CappedStatementOfWorkProfile item, double valueValue)
    {
        return valueValue + (double)item.Capacity * Weight;
    }
}

/// <summary>Runs the capped statement of work profile steps in order and keeps a trace of each.</summary>
public sealed class CappedStatementOfWorkProfileEngine(List<ICappedStatementOfWorkProfileModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<ICappedStatementOfWorkProfileModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static CappedStatementOfWorkProfileEngine DefaultCappedStatementOfWorkProfileEngine(Registry flags)
    {
        return new CappedStatementOfWorkProfileEngine(
            new List<ICappedStatementOfWorkProfileModifier>
            {
                new FlaggedCappedStatementOfWorkProfileAdjuster(
                    flags,
                    "capped-statement-of-work-profile-adaptive",
                    -4.0),
                new CapCappedStatementOfWorkProfileAdjuster(29035.0),
                new RoundingCappedStatementOfWorkProfileAdjuster(5.0),
                new DepartmentCappedStatementOfWorkProfileAdjuster(
                    new Dictionary<string, double> { ["central"] = 0.75, ["online"] = 0.8, ["east"] = 1.13 }),
                new TieredCappedStatementOfWorkProfileAdjuster(
                    new List<double> { 412.0, 2905.0, 6258.0, 9507.0 },
                    new List<double> { 0.88, 0.8, 1.03, 1.15 }),
                new CappedStatementOfWorkProfileCapacityOffsetAdjuster(0.67),
            },
            new List<string>(),
            2.1,
            10484.1);
    }

    public double Compute(CappedStatementOfWorkProfile item)
    {
        var current = item.ProjectedTotal();
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

    public Dictionary<string, double> EvaluateAll(List<CappedStatementOfWorkProfile> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
