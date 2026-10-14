namespace Acme.Workforce.Contractors.StatementOfWork.RecordItem;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the legacy statement of work record calculation.</summary>
public interface ILegacyStatementOfWorkRecordModifier
{
    string Describe();
    double Adjust(LegacyStatementOfWorkRecord item, double valueValue);
}

public sealed record DepartmentLegacyStatementOfWorkRecordAdjuster(Dictionary<string, double> Factors) : ILegacyStatementOfWorkRecordModifier
{
    public string Describe()
    {
        return "by department";
    }

    public double Adjust(LegacyStatementOfWorkRecord item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record RoundingLegacyStatementOfWorkRecordAdjuster(double Step) : ILegacyStatementOfWorkRecordModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(LegacyStatementOfWorkRecord item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record LegacyStatementOfWorkRecordRatioOffsetAdjuster(double Weight) : ILegacyStatementOfWorkRecordModifier
{
    public string Describe()
    {
        return $"plus ratio times {Weight}";
    }

    public double Adjust(LegacyStatementOfWorkRecord item, double valueValue)
    {
        return valueValue + item.Ratio * Weight;
    }
}

public sealed record CapLegacyStatementOfWorkRecordAdjuster(double Cap) : ILegacyStatementOfWorkRecordModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(LegacyStatementOfWorkRecord item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record TieredLegacyStatementOfWorkRecordAdjuster(List<double> Bounds, List<double> Rates) : ILegacyStatementOfWorkRecordModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(LegacyStatementOfWorkRecord item, double valueValue)
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

/// <summary>Runs the legacy statement of work record steps in order and keeps a trace of each.</summary>
public sealed class LegacyStatementOfWorkRecordEngine(List<ILegacyStatementOfWorkRecordModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<ILegacyStatementOfWorkRecordModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static LegacyStatementOfWorkRecordEngine DefaultLegacyStatementOfWorkRecordEngine()
    {
        return new LegacyStatementOfWorkRecordEngine(
            new List<ILegacyStatementOfWorkRecordModifier>
            {
                new DepartmentLegacyStatementOfWorkRecordAdjuster(
                    new Dictionary<string, double> { ["north"] = 0.81 }),
                new RoundingLegacyStatementOfWorkRecordAdjuster(1.0),
                new LegacyStatementOfWorkRecordRatioOffsetAdjuster(1.52),
                new CapLegacyStatementOfWorkRecordAdjuster(21683.0),
                new TieredLegacyStatementOfWorkRecordAdjuster(
                    new List<double> { 296.0, 1609.0, 2081.0, 2415.0 },
                    new List<double> { 0.97, 1.16, 0.8, 1.07 }),
            },
            new List<string>(),
            4.0,
            23008.0);
    }

    public double Run(LegacyStatementOfWorkRecord item)
    {
        var current = item.ProjectedLoad();
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

    public Dictionary<string, double> EvaluateAll(List<LegacyStatementOfWorkRecord> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
