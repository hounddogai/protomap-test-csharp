namespace Acme.Workforce.Orgdesign.Span.Estimate;

using Acme.Workforce.Kit.Text;

/// <summary>Where a span checkpoint is in its life cycle.</summary>
public enum SpanCheckpointStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>A span checkpoint. Its code is unique within a tenant.</summary>
public sealed class SpanCheckpoint(string code, string name, int quantity, int sequence, int windowDays, double score, SpanCheckpointStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int Quantity { get; set; } = quantity;
    public int Sequence { get; set; } = sequence;
    public int WindowDays { get; set; } = windowDays;
    public double Score { get; set; } = score;
    public SpanCheckpointStatus Status { get; set; } = status;

    /// <summary>Starts a span checkpoint in status planned.</summary>
    public static SpanCheckpoint NewSpanCheckpoint(string code, string name)
    {
        return new SpanCheckpoint(code, name, 0, 0, 0, 0.0, SpanCheckpointStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case SpanCheckpointStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(SpanCheckpointStatus next)
    {
        switch (Status)
        {
            case SpanCheckpointStatus.Planned:
                return next == SpanCheckpointStatus.InProgress;
            case SpanCheckpointStatus.Scheduled:
                switch (next)
                {
                    case SpanCheckpointStatus.Completed:
                    case SpanCheckpointStatus.Cancelled:
                        return true;
                }
                return false;
            case SpanCheckpointStatus.InProgress:
                return next == SpanCheckpointStatus.Completed;
            case SpanCheckpointStatus.Completed:
                switch (next)
                {
                    case SpanCheckpointStatus.Cancelled:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(SpanCheckpointStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"span checkpoint {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} / {Name}";
    }

    public double EffectiveTotal()
    {
        return Math.Round((double)Sequence * (double)Quantity, 2);
    }

    public SpanCheckpoint Copy()
    {
        return new SpanCheckpoint(Code, Name, Quantity, Sequence, WindowDays, Score, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
