namespace Acme.Workforce.Orgdesign.Span.Preference;

using Acme.Workforce.Kit.Text;

/// <summary>Where a span record is in its life cycle.</summary>
public enum SpanRecordStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>The record of a span record and its current status.</summary>
public sealed class SpanRecord(string code, string label, string employeeId, int graceDays, int batchSize, string category, bool approved, string phone, string unit, SpanRecordStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string EmployeeId { get; set; } = employeeId;
    public int GraceDays { get; set; } = graceDays;
    public int BatchSize { get; set; } = batchSize;
    public string Category { get; set; } = category;
    public bool Approved { get; set; } = approved;
    public string Phone { get; set; } = phone;
    public string Unit { get; set; } = unit;
    public SpanRecordStatus Status { get; set; } = status;

    /// <summary>Starts a span record in status planned.</summary>
    public static SpanRecord NewSpanRecord(string code, string label)
    {
        return new SpanRecord(code, label, "", 0, 0, "", false, "", "", SpanRecordStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case SpanRecordStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(SpanRecordStatus next)
    {
        switch (Status)
        {
            case SpanRecordStatus.Planned:
                return next == SpanRecordStatus.Cancelled;
            case SpanRecordStatus.Scheduled:
                return next == SpanRecordStatus.InProgress;
            case SpanRecordStatus.InProgress:
                switch (next)
                {
                    case SpanRecordStatus.Completed:
                    case SpanRecordStatus.Cancelled:
                        return true;
                }
                return false;
            case SpanRecordStatus.Completed:
                return next == SpanRecordStatus.Cancelled;
        }
        return false;
    }

    public void MoveTo(SpanRecordStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"span record {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{"span record"} {Code}";
    }

    public double NetValue()
    {
        return Math.Round((double)BatchSize * (double)GraceDays, 2);
    }

    public SpanRecord Copy()
    {
        return new SpanRecord(
            Code,
            Label,
            EmployeeId,
            GraceDays,
            BatchSize,
            Category,
            Approved,
            Phone,
            Unit,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Category);
    }
}
