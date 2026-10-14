namespace Acme.Workforce.Workplace.FloorPlan.Baseline;

using Acme.Workforce.Kit.Text;

/// <summary>Where a floor plan estimate is in its life cycle.</summary>
public enum FloorPlanEstimateStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>The record of a floor plan estimate and its current status.</summary>
public sealed class FloorPlanEstimate(string code, string label, int version, string locale, string summary, string managerId, int sequence, FloorPlanEstimateStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int Version { get; set; } = version;
    public string Locale { get; set; } = locale;
    public string Summary { get; set; } = summary;
    public string ManagerId { get; set; } = managerId;
    public int Sequence { get; set; } = sequence;
    public FloorPlanEstimateStatus Status { get; set; } = status;

    /// <summary>Starts a floor plan estimate in status draft.</summary>
    public static FloorPlanEstimate NewFloorPlanEstimate(string code, string label)
    {
        return new FloorPlanEstimate(code, label, 0, "", "", "", 0, FloorPlanEstimateStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case FloorPlanEstimateStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(FloorPlanEstimateStatus next)
    {
        switch (Status)
        {
            case FloorPlanEstimateStatus.Draft:
                switch (next)
                {
                    case FloorPlanEstimateStatus.Active:
                        return true;
                }
                return false;
            case FloorPlanEstimateStatus.Pending:
                return next == FloorPlanEstimateStatus.Suspended || next == FloorPlanEstimateStatus.Closed;
            case FloorPlanEstimateStatus.Active:
                switch (next)
                {
                    case FloorPlanEstimateStatus.Closed:
                        return true;
                }
                return false;
            case FloorPlanEstimateStatus.Suspended:
                return next == FloorPlanEstimateStatus.Closed;
        }
        return false;
    }

    public void MoveTo(FloorPlanEstimateStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"floor plan estimate {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Version += 1;
    }

    public string DisplayLabel()
    {
        return $"{Code} ({Label})";
    }

    public double NetVolume()
    {
        return Math.Round((double)Version + (double)Sequence, 2);
    }

    public FloorPlanEstimate Copy()
    {
        return new FloorPlanEstimate(Code, Label, Version, Locale, Summary, ManagerId, Sequence, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Locale);
    }
}
