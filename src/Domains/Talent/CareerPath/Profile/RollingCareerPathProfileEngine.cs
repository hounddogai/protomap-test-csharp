namespace Acme.Workforce.Talent.CareerPath.Profile;

/// <summary>One step of the rolling career path profile calculation.</summary>
public interface IRollingCareerPathProfileAdjuster
{
    string Describe();
    double Adjust(RollingCareerPathProfile item, double valueValue);
}

public sealed record TieredRollingCareerPathProfileAdjuster(List<double> Bounds, List<double> Rates) : IRollingCareerPathProfileAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(RollingCareerPathProfile item, double valueValue)
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

public sealed record RollingCareerPathProfileWindowDaysOffsetAdjuster(double Weight) : IRollingCareerPathProfileAdjuster
{
    public string Describe()
    {
        return $"plus window days times {Weight}";
    }

    public double Adjust(RollingCareerPathProfile item, double valueValue)
    {
        return valueValue + (double)item.WindowDays * Weight;
    }
}

public sealed record CapRollingCareerPathProfileAdjuster(double Cap) : IRollingCareerPathProfileAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(RollingCareerPathProfile item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

/// <summary>Runs the rolling career path profile steps in order and keeps a trace of each.</summary>
public sealed class RollingCareerPathProfileEngine(List<IRollingCareerPathProfileAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IRollingCareerPathProfileAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static RollingCareerPathProfileEngine DefaultRollingCareerPathProfileEngine()
    {
        return new RollingCareerPathProfileEngine(
            new List<IRollingCareerPathProfileAdjuster>
            {
                new TieredRollingCareerPathProfileAdjuster(
                    new List<double> { 133.0, 7702.0, 8526.0 },
                    new List<double> { 1.01, 0.96, 0.93 }),
                new RollingCareerPathProfileWindowDaysOffsetAdjuster(0.21),
                new CapRollingCareerPathProfileAdjuster(14384.0),
            },
            new List<string>(),
            4.3,
            43533.7);
    }

    public double Run(RollingCareerPathProfile item)
    {
        var current = item.WeightedLoad();
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

    public Dictionary<string, double> EvaluateAll(List<RollingCareerPathProfile> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
