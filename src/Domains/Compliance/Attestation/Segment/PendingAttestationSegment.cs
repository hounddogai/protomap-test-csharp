namespace Acme.Workforce.Compliance.Attestation.Segment;

using Acme.Workforce.Kit.Text;

/// <summary>Where a pending attestation segment is in its life cycle.</summary>
public enum PendingAttestationSegmentStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>The record of a pending attestation segment and its current status.</summary>
public sealed class PendingAttestationSegment(string code, string title, int windowDays, double amount, bool pinned, PendingAttestationSegmentStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int WindowDays { get; set; } = windowDays;
    public double Amount { get; set; } = amount;
    public bool Pinned { get; set; } = pinned;
    public PendingAttestationSegmentStatus Status { get; set; } = status;

    /// <summary>Starts a pending attestation segment in status new.</summary>
    public static PendingAttestationSegment NewPendingAttestationSegment(string code, string title)
    {
        return new PendingAttestationSegment(code, title, 0, 0.0, false, PendingAttestationSegmentStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PendingAttestationSegmentStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PendingAttestationSegmentStatus next)
    {
        switch (Status)
        {
            case PendingAttestationSegmentStatus.New:
                switch (next)
                {
                    case PendingAttestationSegmentStatus.Queued:
                    case PendingAttestationSegmentStatus.Failed:
                        return true;
                }
                return false;
            case PendingAttestationSegmentStatus.Queued:
                switch (next)
                {
                    case PendingAttestationSegmentStatus.Running:
                    case PendingAttestationSegmentStatus.Failed:
                        return true;
                }
                return false;
            case PendingAttestationSegmentStatus.Running:
                switch (next)
                {
                    case PendingAttestationSegmentStatus.Done:
                        return true;
                }
                return false;
            case PendingAttestationSegmentStatus.Done:
                return next == PendingAttestationSegmentStatus.Failed;
        }
        return false;
    }

    public void MoveTo(PendingAttestationSegmentStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"pending attestation segment {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code}: {Title}";
    }

    public double ProjectedTotal()
    {
        return Math.Round(Amount + (double)WindowDays, 2);
    }

    public PendingAttestationSegment Copy()
    {
        return new PendingAttestationSegment(Code, Title, WindowDays, Amount, Pinned, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
