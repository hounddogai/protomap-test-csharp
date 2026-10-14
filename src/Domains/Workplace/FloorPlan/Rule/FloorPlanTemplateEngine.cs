namespace Acme.Workforce.Workplace.FloorPlan.Rule;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the floor plan template calculation.</summary>
public interface IFloorPlanTemplateFactor
{
    string Describe();
    double Adjust(FloorPlanTemplate item, double valueValue);
}

public sealed record LabelFloorPlanTemplateAdjuster(Dictionary<string, double> Factors) : IFloorPlanTemplateFactor
{
    public string Describe()
    {
        return "by label";
    }

    public double Adjust(FloorPlanTemplate item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record FlaggedFloorPlanTemplateAdjuster(Registry Flags, string Flag, double Percent) : IFloorPlanTemplateFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(FloorPlanTemplate item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record FloorPlanTemplateEndsAtOffsetAdjuster(double Weight) : IFloorPlanTemplateFactor
{
    public string Describe()
    {
        return $"plus ends at times {Weight}";
    }

    public double Adjust(FloorPlanTemplate item, double valueValue)
    {
        return valueValue + (double)item.EndsAt * Weight;
    }
}

public sealed record MinimumFloorPlanTemplateAdjuster(double Minimum) : IFloorPlanTemplateFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(FloorPlanTemplate item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record RoundingFloorPlanTemplateAdjuster(double Step) : IFloorPlanTemplateFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(FloorPlanTemplate item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record TieredFloorPlanTemplateAdjuster(List<double> Bounds, List<double> Rates) : IFloorPlanTemplateFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(FloorPlanTemplate item, double valueValue)
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

/// <summary>Runs the floor plan template steps in order and keeps a trace of each.</summary>
public sealed class FloorPlanTemplateEngine(List<IFloorPlanTemplateFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IFloorPlanTemplateFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static FloorPlanTemplateEngine DefaultFloorPlanTemplateEngine(Registry flags)
    {
        return new FloorPlanTemplateEngine(
            new List<IFloorPlanTemplateFactor>
            {
                new LabelFloorPlanTemplateAdjuster(
                    new Dictionary<string, double> { ["east"] = 1.31, ["central"] = 0.84, ["south"] = 0.95 }),
                new FlaggedFloorPlanTemplateAdjuster(flags, "floor-plan-template-rolling", 8.2),
                new FloorPlanTemplateEndsAtOffsetAdjuster(0.15),
                new MinimumFloorPlanTemplateAdjuster(46.8),
                new RoundingFloorPlanTemplateAdjuster(1.0),
                new TieredFloorPlanTemplateAdjuster(
                    new List<double> { 1187.0, 4549.0, 7734.0, 9246.0 },
                    new List<double> { 0.94, 0.81, 1.18, 1.07 }),
            },
            new List<string>(),
            0.9,
            44759.5);
    }

    public double Evaluate(FloorPlanTemplate item)
    {
        var current = item.AdjustedVolume();
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

    public Dictionary<string, double> EvaluateAll(List<FloorPlanTemplate> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
