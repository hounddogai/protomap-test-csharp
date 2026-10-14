namespace Acme.Workforce.Assets.Assignment.Estimate;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the assignment manifest calculation.</summary>
public interface IAssignmentManifestFactor
{
    string Describe();
    double Adjust(AssignmentManifest item, double valueValue);
}

public sealed record RoundingAssignmentManifestAdjuster(double Step) : IAssignmentManifestFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(AssignmentManifest item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record TieredAssignmentManifestAdjuster(List<double> Bounds, List<double> Rates) : IAssignmentManifestFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(AssignmentManifest item, double valueValue)
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

public sealed record AssignmentManifestHoursOffsetAdjuster(double Weight) : IAssignmentManifestFactor
{
    public string Describe()
    {
        return $"plus hours times {Weight}";
    }

    public double Adjust(AssignmentManifest item, double valueValue)
    {
        return valueValue + item.Hours * Weight;
    }
}

public sealed record FlaggedAssignmentManifestAdjuster(Registry Flags, string Flag, double Percent) : IAssignmentManifestFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(AssignmentManifest item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the assignment manifest steps in order and keeps a trace of each.</summary>
public sealed class AssignmentManifestEngine(List<IAssignmentManifestFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IAssignmentManifestFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static AssignmentManifestEngine DefaultAssignmentManifestEngine(Registry flags)
    {
        return new AssignmentManifestEngine(
            new List<IAssignmentManifestFactor>
            {
                new RoundingAssignmentManifestAdjuster(1.0),
                new TieredAssignmentManifestAdjuster(
                    new List<double> { 4877.0, 5076.0, 5297.0 },
                    new List<double> { 1.13, 1.07, 1.05 }),
                new AssignmentManifestHoursOffsetAdjuster(0.23),
                new FlaggedAssignmentManifestAdjuster(flags, "assignment-manifest-regional", -1.1),
            },
            new List<string>(),
            4.0,
            87288.8);
    }

    public double Run(AssignmentManifest item)
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

    public Dictionary<string, double> EvaluateAll(List<AssignmentManifest> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
