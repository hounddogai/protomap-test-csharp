namespace Acme.Workforce.Orgdesign.Span.Template;

using Acme.Workforce.Kit.Text;

/// <summary>Where a weighted span mapping is in its life cycle.</summary>
public enum WeightedSpanMappingStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>The record of a weighted span mapping and its current status.</summary>
public sealed class WeightedSpanMapping(string code, string name, string salaryBand, int capacity, bool pinned, string department, double floorValue, string source, WeightedSpanMappingStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string SalaryBand { get; set; } = salaryBand;
    public int Capacity { get; set; } = capacity;
    public bool Pinned { get; set; } = pinned;
    public string Department { get; set; } = department;
    public double FloorValue { get; set; } = floorValue;
    public string Source { get; set; } = source;
    public WeightedSpanMappingStatus Status { get; set; } = status;

    /// <summary>Starts a weighted span mapping in status open.</summary>
    public static WeightedSpanMapping NewWeightedSpanMapping(string code, string name)
    {
        return new WeightedSpanMapping(code, name, "", 0, false, "", 0.0, "", WeightedSpanMappingStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case WeightedSpanMappingStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(WeightedSpanMappingStatus next)
    {
        switch (Status)
        {
            case WeightedSpanMappingStatus.Open:
                return next == WeightedSpanMappingStatus.Resolved;
            case WeightedSpanMappingStatus.InReview:
                return next == WeightedSpanMappingStatus.Reopened;
            case WeightedSpanMappingStatus.Resolved:
                return next == WeightedSpanMappingStatus.Archived;
            case WeightedSpanMappingStatus.Reopened:
                switch (next)
                {
                    case WeightedSpanMappingStatus.Archived:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(WeightedSpanMappingStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"weighted span mapping {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code}: {Name}";
    }

    public double WeightedLoad()
    {
        return Math.Round(FloorValue * (double)Capacity, 2);
    }

    public WeightedSpanMapping Copy()
    {
        return new WeightedSpanMapping(
            Code,
            Name,
            SalaryBand,
            Capacity,
            Pinned,
            Department,
            FloorValue,
            Source,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Department);
    }
}
