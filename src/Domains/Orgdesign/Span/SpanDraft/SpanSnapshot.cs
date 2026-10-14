namespace Acme.Workforce.Orgdesign.Span.SpanDraft;

using Acme.Workforce.Kit.Text;

/// <summary>Where a span snapshot is in its life cycle.</summary>
public enum SpanSnapshotStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>A span snapshot. Its code is unique within a tenant.</summary>
public sealed class SpanSnapshot(string code, string label, int sequence, int capacity, string name, string phone, bool approved, int windowDays, SpanSnapshotStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int Sequence { get; set; } = sequence;
    public int Capacity { get; set; } = capacity;
    public string Name { get; set; } = name;
    public string Phone { get; set; } = phone;
    public bool Approved { get; set; } = approved;
    public int WindowDays { get; set; } = windowDays;
    public SpanSnapshotStatus Status { get; set; } = status;

    /// <summary>Starts a span snapshot in status planned.</summary>
    public static SpanSnapshot NewSpanSnapshot(string code, string label)
    {
        return new SpanSnapshot(code, label, 0, 0, "", "", false, 0, SpanSnapshotStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case SpanSnapshotStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(SpanSnapshotStatus next)
    {
        switch (Status)
        {
            case SpanSnapshotStatus.Planned:
                return next == SpanSnapshotStatus.Scheduled;
            case SpanSnapshotStatus.Scheduled:
                switch (next)
                {
                    case SpanSnapshotStatus.InProgress:
                    case SpanSnapshotStatus.Completed:
                        return true;
                }
                return false;
            case SpanSnapshotStatus.InProgress:
                return next == SpanSnapshotStatus.Completed || next == SpanSnapshotStatus.Cancelled;
            case SpanSnapshotStatus.Completed:
                return next == SpanSnapshotStatus.Cancelled;
        }
        return false;
    }

    public void MoveTo(SpanSnapshotStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"span snapshot {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{"span snapshot"} {Code}";
    }

    public double NetLoad()
    {
        return Math.Round((double)WindowDays * (double)Capacity, 2);
    }

    public SpanSnapshot Copy()
    {
        return new SpanSnapshot(Code, Label, Sequence, Capacity, Name, Phone, Approved, WindowDays, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
