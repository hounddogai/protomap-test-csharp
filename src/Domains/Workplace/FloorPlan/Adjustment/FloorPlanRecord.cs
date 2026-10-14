namespace Acme.Workforce.Workplace.FloorPlan.Adjustment;

using Acme.Workforce.Kit.Text;

/// <summary>Where a floor plan record is in its life cycle.</summary>
public enum FloorPlanRecordStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>One floor plan record, with what the planners need to act on it.</summary>
public sealed class FloorPlanRecord(string code, string name, int sequence, string contactEmail, int batchSize, string source, int limit, int endsAt, double ceiling, int durationMinutes, FloorPlanRecordStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int Sequence { get; set; } = sequence;
    public string ContactEmail { get; set; } = contactEmail;
    public int BatchSize { get; set; } = batchSize;
    public string Source { get; set; } = source;
    public int Limit { get; set; } = limit;
    public int EndsAt { get; set; } = endsAt;
    public double Ceiling { get; set; } = ceiling;
    public int DurationMinutes { get; set; } = durationMinutes;
    public FloorPlanRecordStatus Status { get; set; } = status;

    /// <summary>Starts a floor plan record in status open.</summary>
    public static FloorPlanRecord NewFloorPlanRecord(string code, string name)
    {
        return new FloorPlanRecord(code, name, 0, "", 0, "", 0, 0, 0.0, 0, FloorPlanRecordStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case FloorPlanRecordStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(FloorPlanRecordStatus next)
    {
        switch (Status)
        {
            case FloorPlanRecordStatus.Open:
                switch (next)
                {
                    case FloorPlanRecordStatus.Reopened:
                    case FloorPlanRecordStatus.Archived:
                        return true;
                }
                return false;
            case FloorPlanRecordStatus.InReview:
                switch (next)
                {
                    case FloorPlanRecordStatus.Reopened:
                    case FloorPlanRecordStatus.Archived:
                        return true;
                }
                return false;
            case FloorPlanRecordStatus.Resolved:
                switch (next)
                {
                    case FloorPlanRecordStatus.Reopened:
                    case FloorPlanRecordStatus.Archived:
                        return true;
                }
                return false;
            case FloorPlanRecordStatus.Reopened:
                return next == FloorPlanRecordStatus.Archived;
        }
        return false;
    }

    public void MoveTo(FloorPlanRecordStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"floor plan record {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        EndsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} ({Name})";
    }

    public double WeightedVolume()
    {
        return Math.Round((double)Sequence - (double)Limit, 2);
    }

    public FloorPlanRecord Copy()
    {
        return new FloorPlanRecord(
            Code,
            Name,
            Sequence,
            ContactEmail,
            BatchSize,
            Source,
            Limit,
            EndsAt,
            Ceiling,
            DurationMinutes,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Source);
    }
}
