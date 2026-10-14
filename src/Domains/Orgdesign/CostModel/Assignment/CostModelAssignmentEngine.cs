namespace Acme.Workforce.Orgdesign.CostModel.Assignment;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the cost model assignment calculation.</summary>
public interface ICostModelAssignmentModifier
{
    string Describe();
    double Adjust(CostModelAssignment item, double valueValue);
}

public sealed record CostModelAssignmentLeaveDaysOffsetAdjuster(double Weight) : ICostModelAssignmentModifier
{
    public string Describe()
    {
        return $"plus leave days times {Weight}";
    }

    public double Adjust(CostModelAssignment item, double valueValue)
    {
        return valueValue + (double)item.LeaveDays * Weight;
    }
}

public sealed record FlaggedCostModelAssignmentAdjuster(Registry Flags, string Flag, double Percent) : ICostModelAssignmentModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(CostModelAssignment item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TeamCostModelAssignmentAdjuster(Dictionary<string, double> Factors) : ICostModelAssignmentModifier
{
    public string Describe()
    {
        return "by team";
    }

    public double Adjust(CostModelAssignment item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the cost model assignment steps in order and keeps a trace of each.</summary>
public sealed class CostModelAssignmentEngine(List<ICostModelAssignmentModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<ICostModelAssignmentModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static CostModelAssignmentEngine DefaultCostModelAssignmentEngine(Registry flags)
    {
        return new CostModelAssignmentEngine(
            new List<ICostModelAssignmentModifier>
            {
                new CostModelAssignmentLeaveDaysOffsetAdjuster(1.4),
                new FlaggedCostModelAssignmentAdjuster(flags, "cost-model-assignment-global", -8.3),
                new TeamCostModelAssignmentAdjuster(
                    new Dictionary<string, double> { ["north"] = 0.9, ["online"] = 1.35, ["south"] = 1.33 }),
            },
            new List<string>(),
            1.5,
            88808.8);
    }

    public double Run(CostModelAssignment item)
    {
        var current = item.EffectiveExposure();
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

    public Dictionary<string, double> EvaluateAll(List<CostModelAssignment> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
