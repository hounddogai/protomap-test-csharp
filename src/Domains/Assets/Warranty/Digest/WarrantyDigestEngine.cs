namespace Acme.Workforce.Assets.Warranty.Digest;

/// <summary>One step of the warranty digest calculation.</summary>
public interface IWarrantyDigestModifier
{
    string Describe();
    double Adjust(WarrantyDigest item, double valueValue);
}

public sealed record CapWarrantyDigestAdjuster(double Cap) : IWarrantyDigestModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(WarrantyDigest item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record LabelWarrantyDigestAdjuster(Dictionary<string, double> Factors) : IWarrantyDigestModifier
{
    public string Describe()
    {
        return "by label";
    }

    public double Adjust(WarrantyDigest item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record MinimumWarrantyDigestAdjuster(double Minimum) : IWarrantyDigestModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(WarrantyDigest item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record PercentWarrantyDigestAdjuster(double Percent) : IWarrantyDigestModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(WarrantyDigest item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the warranty digest steps in order and keeps a trace of each.</summary>
public sealed class WarrantyDigestEngine(List<IWarrantyDigestModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IWarrantyDigestModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static WarrantyDigestEngine DefaultWarrantyDigestEngine()
    {
        return new WarrantyDigestEngine(
            new List<IWarrantyDigestModifier>
            {
                new CapWarrantyDigestAdjuster(45691.0),
                new LabelWarrantyDigestAdjuster(
                    new Dictionary<string, double> { ["retail"] = 1.19, ["east"] = 1.36, ["online"] = 1.11 }),
                new MinimumWarrantyDigestAdjuster(26.2),
                new PercentWarrantyDigestAdjuster(-11.3),
            },
            new List<string>(),
            3.1,
            83775.1);
    }

    public double Evaluate(WarrantyDigest item)
    {
        var current = item.ProjectedLoad();
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

    public Dictionary<string, double> EvaluateAll(List<WarrantyDigest> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
