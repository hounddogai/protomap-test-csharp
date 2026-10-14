namespace Acme.Workforce.Compensation.PayBand.Milestone;

using Acme.Workforce.Kit.Text;

/// <summary>Where a primary pay band checkpoint is in its life cycle.</summary>
public enum PrimaryPayBandCheckpointStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>A primary pay band checkpoint. Its code is unique within a tenant.</summary>
public sealed class PrimaryPayBandCheckpoint(string code, string label, int leaveDays, string phone, string badge, PrimaryPayBandCheckpointStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int LeaveDays { get; set; } = leaveDays;
    public string Phone { get; set; } = phone;
    public string Badge { get; set; } = badge;
    public PrimaryPayBandCheckpointStatus Status { get; set; } = status;

    /// <summary>Starts a primary pay band checkpoint in status new.</summary>
    public static PrimaryPayBandCheckpoint NewPrimaryPayBandCheckpoint(string code, string label)
    {
        return new PrimaryPayBandCheckpoint(code, label, 0, "", "", PrimaryPayBandCheckpointStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PrimaryPayBandCheckpointStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PrimaryPayBandCheckpointStatus next)
    {
        switch (Status)
        {
            case PrimaryPayBandCheckpointStatus.New:
                switch (next)
                {
                    case PrimaryPayBandCheckpointStatus.Queued:
                        return true;
                }
                return false;
            case PrimaryPayBandCheckpointStatus.Queued:
                return next == PrimaryPayBandCheckpointStatus.Failed;
            case PrimaryPayBandCheckpointStatus.Running:
                return next == PrimaryPayBandCheckpointStatus.Done || next == PrimaryPayBandCheckpointStatus.Failed;
            case PrimaryPayBandCheckpointStatus.Done:
                return next == PrimaryPayBandCheckpointStatus.Failed;
        }
        return false;
    }

    public void MoveTo(PrimaryPayBandCheckpointStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"primary pay band checkpoint {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} / {Label}";
    }

    public double MeasuredValue()
    {
        return (double)LeaveDays;
    }

    public PrimaryPayBandCheckpoint Copy()
    {
        return new PrimaryPayBandCheckpoint(Code, Label, LeaveDays, Phone, Badge, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
