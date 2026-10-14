namespace Acme.Workforce.Absence.ReturnPlan.Policy;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the return plan record calculation.</summary>
public interface IReturnPlanRecordStep
{
    string Describe();
    double Adjust(ReturnPlanRecord item, double valueValue);
}

public sealed record FlaggedReturnPlanRecordAdjuster(Registry Flags, string Flag, double Percent) : IReturnPlanRecordStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(ReturnPlanRecord item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapReturnPlanRecordAdjuster(double Cap) : IReturnPlanRecordStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(ReturnPlanRecord item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record RoundingReturnPlanRecordAdjuster(double Step) : IReturnPlanRecordStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(ReturnPlanRecord item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record TieredReturnPlanRecordAdjuster(List<double> Bounds, List<double> Rates) : IReturnPlanRecordStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(ReturnPlanRecord item, double valueValue)
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

public sealed record ReturnPlanRecordGraceDaysOffsetAdjuster(double Weight) : IReturnPlanRecordStep
{
    public string Describe()
    {
        return $"plus grace days times {Weight}";
    }

    public double Adjust(ReturnPlanRecord item, double valueValue)
    {
        return valueValue + (double)item.GraceDays * Weight;
    }
}

/// <summary>Runs the return plan record steps in order and keeps a trace of each.</summary>
public sealed class ReturnPlanRecordEngine(List<IReturnPlanRecordStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IReturnPlanRecordStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static ReturnPlanRecordEngine DefaultReturnPlanRecordEngine(Registry flags)
    {
        return new ReturnPlanRecordEngine(
            new List<IReturnPlanRecordStep>
            {
                new FlaggedReturnPlanRecordAdjuster(flags, "return-plan-record-batched", -0.4),
                new CapReturnPlanRecordAdjuster(45355.0),
                new RoundingReturnPlanRecordAdjuster(0.1),
                new TieredReturnPlanRecordAdjuster(
                    new List<double> { 758.0, 9276.0 },
                    new List<double> { 1.18, 0.81 }),
                new ReturnPlanRecordGraceDaysOffsetAdjuster(1.93),
            },
            new List<string>(),
            1.7,
            48420.9);
    }

    public double Run(ReturnPlanRecord item)
    {
        var current = item.NetVolume();
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

    public Dictionary<string, double> EvaluateAll(List<ReturnPlanRecord> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
