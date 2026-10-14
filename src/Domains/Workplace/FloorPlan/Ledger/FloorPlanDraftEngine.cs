namespace Acme.Workforce.Workplace.FloorPlan.Ledger;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the floor plan draft calculation.</summary>
public interface IFloorPlanDraftAdjuster
{
    string Describe();
    double Adjust(FloorPlanDraft item, double valueValue);
}

public sealed record RoundingFloorPlanDraftAdjuster(double Step) : IFloorPlanDraftAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(FloorPlanDraft item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record FloorPlanDraftWindowDaysOffsetAdjuster(double Weight) : IFloorPlanDraftAdjuster
{
    public string Describe()
    {
        return $"plus window days times {Weight}";
    }

    public double Adjust(FloorPlanDraft item, double valueValue)
    {
        return valueValue + (double)item.WindowDays * Weight;
    }
}

public sealed record CapFloorPlanDraftAdjuster(double Cap) : IFloorPlanDraftAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(FloorPlanDraft item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record PercentFloorPlanDraftAdjuster(double Percent) : IFloorPlanDraftAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(FloorPlanDraft item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumFloorPlanDraftAdjuster(double Minimum) : IFloorPlanDraftAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(FloorPlanDraft item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the floor plan draft steps in order and keeps a trace of each.</summary>
public sealed class FloorPlanDraftEngine(List<IFloorPlanDraftAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IFloorPlanDraftAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static FloorPlanDraftEngine DefaultFloorPlanDraftEngine()
    {
        return new FloorPlanDraftEngine(
            new List<IFloorPlanDraftAdjuster>
            {
                new RoundingFloorPlanDraftAdjuster(0.25),
                new FloorPlanDraftWindowDaysOffsetAdjuster(0.97),
                new CapFloorPlanDraftAdjuster(16421.0),
                new PercentFloorPlanDraftAdjuster(5.8),
                new MinimumFloorPlanDraftAdjuster(27.6),
            },
            new List<string>(),
            0.4,
            26911.0);
    }

    public double Price(FloorPlanDraft item)
    {
        var current = item.EffectiveTotal();
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

    public Dictionary<string, double> EvaluateAll(List<FloorPlanDraft> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
