namespace Acme.Workforce.Orgdesign.Span.RecordItem;

using Acme.Workforce.Kit.Text;

/// <summary>Where a legacy span record is in its life cycle.</summary>
public enum LegacySpanRecordStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>A legacy span record. Its code is unique within a tenant.</summary>
public sealed class LegacySpanRecord(string code, string title, int quantity, string region, double cost, string managerId, int endsAt, LegacySpanRecordStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int Quantity { get; set; } = quantity;
    public string Region { get; set; } = region;
    public double Cost { get; set; } = cost;
    public string ManagerId { get; set; } = managerId;
    public int EndsAt { get; set; } = endsAt;
    public LegacySpanRecordStatus Status { get; set; } = status;

    /// <summary>Starts a legacy span record in status planned.</summary>
    public static LegacySpanRecord NewLegacySpanRecord(string code, string title)
    {
        return new LegacySpanRecord(code, title, 0, "", 0.0, "", 0, LegacySpanRecordStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LegacySpanRecordStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LegacySpanRecordStatus next)
    {
        switch (Status)
        {
            case LegacySpanRecordStatus.Planned:
                return next == LegacySpanRecordStatus.InProgress || next == LegacySpanRecordStatus.Cancelled;
            case LegacySpanRecordStatus.Scheduled:
                switch (next)
                {
                    case LegacySpanRecordStatus.InProgress:
                    case LegacySpanRecordStatus.Completed:
                        return true;
                }
                return false;
            case LegacySpanRecordStatus.InProgress:
                return next == LegacySpanRecordStatus.Cancelled;
            case LegacySpanRecordStatus.Completed:
                return next == LegacySpanRecordStatus.Cancelled;
        }
        return false;
    }

    public void MoveTo(LegacySpanRecordStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"legacy span record {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        EndsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{"legacy span record"} {Code}";
    }

    public double AdjustedExposure()
    {
        return Math.Round((double)EndsAt * (double)Quantity, 2);
    }

    public LegacySpanRecord Copy()
    {
        return new LegacySpanRecord(Code, Title, Quantity, Region, Cost, ManagerId, EndsAt, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Region);
    }
}
