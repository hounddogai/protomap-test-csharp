namespace Acme.Workforce.Orgdesign.Headcount.Assignment;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the headcount audit calculation.</summary>
public interface IHeadcountAuditFactor
{
    string Describe();
    double Adjust(HeadcountAudit item, double valueValue);
}

public sealed record MinimumHeadcountAuditAdjuster(double Minimum) : IHeadcountAuditFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(HeadcountAudit item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record CapHeadcountAuditAdjuster(double Cap) : IHeadcountAuditFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(HeadcountAudit item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record RoundingHeadcountAuditAdjuster(double Step) : IHeadcountAuditFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(HeadcountAudit item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record HeadcountAuditScoreOffsetAdjuster(double Weight) : IHeadcountAuditFactor
{
    public string Describe()
    {
        return $"plus score times {Weight}";
    }

    public double Adjust(HeadcountAudit item, double valueValue)
    {
        return valueValue + item.Score * Weight;
    }
}

/// <summary>Runs the headcount audit steps in order and keeps a trace of each.</summary>
public sealed class HeadcountAuditEngine(List<IHeadcountAuditFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IHeadcountAuditFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static HeadcountAuditEngine DefaultHeadcountAuditEngine()
    {
        return new HeadcountAuditEngine(
            new List<IHeadcountAuditFactor>
            {
                new MinimumHeadcountAuditAdjuster(41.5),
                new CapHeadcountAuditAdjuster(19696.0),
                new RoundingHeadcountAuditAdjuster(5.0),
                new HeadcountAuditScoreOffsetAdjuster(0.16),
            },
            new List<string>(),
            1.7,
            61599.9);
    }

    public double Evaluate(HeadcountAudit item)
    {
        var current = item.AdjustedLoad();
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

    public Dictionary<string, double> EvaluateAll(List<HeadcountAudit> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
