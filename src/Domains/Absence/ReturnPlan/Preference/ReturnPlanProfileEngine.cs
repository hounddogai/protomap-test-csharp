namespace Acme.Workforce.Absence.ReturnPlan.Preference;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the return plan profile calculation.</summary>
public interface IReturnPlanProfileFactor
{
    string Describe();
    double Adjust(ReturnPlanProfile item, double valueValue);
}

public sealed record ReturnPlanProfileFloorValueOffsetAdjuster(double Weight) : IReturnPlanProfileFactor
{
    public string Describe()
    {
        return $"plus floor value times {Weight}";
    }

    public double Adjust(ReturnPlanProfile item, double valueValue)
    {
        return valueValue + item.FloorValue * Weight;
    }
}

public sealed record FlaggedReturnPlanProfileAdjuster(Registry Flags, string Flag, double Percent) : IReturnPlanProfileFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(ReturnPlanProfile item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredReturnPlanProfileAdjuster(List<double> Bounds, List<double> Rates) : IReturnPlanProfileFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(ReturnPlanProfile item, double valueValue)
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

public sealed record SourceReturnPlanProfileAdjuster(Dictionary<string, double> Factors) : IReturnPlanProfileFactor
{
    public string Describe()
    {
        return "by source";
    }

    public double Adjust(ReturnPlanProfile item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the return plan profile steps in order and keeps a trace of each.</summary>
public sealed class ReturnPlanProfileEngine(List<IReturnPlanProfileFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IReturnPlanProfileFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static ReturnPlanProfileEngine DefaultReturnPlanProfileEngine(Registry flags)
    {
        return new ReturnPlanProfileEngine(
            new List<IReturnPlanProfileFactor>
            {
                new ReturnPlanProfileFloorValueOffsetAdjuster(0.24),
                new FlaggedReturnPlanProfileAdjuster(flags, "return-plan-profile-global", 0.7),
                new TieredReturnPlanProfileAdjuster(
                    new List<double> { 7203.0, 7578.0 },
                    new List<double> { 1.1, 1.09 }),
                new SourceReturnPlanProfileAdjuster(new Dictionary<string, double> { ["north"] = 1.05 }),
            },
            new List<string>(),
            1.2,
            90913.4);
    }

    public double Run(ReturnPlanProfile item)
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

    public Dictionary<string, double> EvaluateAll(List<ReturnPlanProfile> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
