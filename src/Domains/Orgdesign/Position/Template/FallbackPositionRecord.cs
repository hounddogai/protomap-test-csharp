namespace Acme.Workforce.Orgdesign.Position.Template;

using Acme.Workforce.Kit.Text;

/// <summary>Where a fallback position record is in its life cycle.</summary>
public enum FallbackPositionRecordStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>A fallback position record. Its code is unique within a tenant.</summary>
public sealed class FallbackPositionRecord(string code, string label, string notes, double floorValue, string skill, string category, FallbackPositionRecordStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Notes { get; set; } = notes;
    public double FloorValue { get; set; } = floorValue;
    public string Skill { get; set; } = skill;
    public string Category { get; set; } = category;
    public FallbackPositionRecordStatus Status { get; set; } = status;

    /// <summary>Starts a fallback position record in status planned.</summary>
    public static FallbackPositionRecord NewFallbackPositionRecord(string code, string label)
    {
        return new FallbackPositionRecord(code, label, "", 0.0, "", "", FallbackPositionRecordStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case FallbackPositionRecordStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(FallbackPositionRecordStatus next)
    {
        switch (Status)
        {
            case FallbackPositionRecordStatus.Planned:
                switch (next)
                {
                    case FallbackPositionRecordStatus.Completed:
                    case FallbackPositionRecordStatus.Cancelled:
                        return true;
                }
                return false;
            case FallbackPositionRecordStatus.Scheduled:
                return next == FallbackPositionRecordStatus.InProgress || next == FallbackPositionRecordStatus.Completed;
            case FallbackPositionRecordStatus.InProgress:
                return next == FallbackPositionRecordStatus.Cancelled;
            case FallbackPositionRecordStatus.Completed:
                return next == FallbackPositionRecordStatus.Cancelled;
        }
        return false;
    }

    public void MoveTo(FallbackPositionRecordStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"fallback position record {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code}: {Label}";
    }

    public double MeasuredValue()
    {
        return FloorValue;
    }

    public FallbackPositionRecord Copy()
    {
        return new FallbackPositionRecord(Code, Label, Notes, FloorValue, Skill, Category, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Category);
    }
}
