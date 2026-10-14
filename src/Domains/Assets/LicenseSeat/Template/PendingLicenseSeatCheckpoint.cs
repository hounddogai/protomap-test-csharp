namespace Acme.Workforce.Assets.LicenseSeat.Template;

using Acme.Workforce.Kit.Text;

/// <summary>Where a pending license seat checkpoint is in its life cycle.</summary>
public enum PendingLicenseSeatCheckpointStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>A pending license seat checkpoint as the license seat team keeps it.</summary>
public sealed class PendingLicenseSeatCheckpoint(string code, string name, int batchSize, int windowDays, string summary, string contactEmail, double amount, double hours, string segment, string externalRef, PendingLicenseSeatCheckpointStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int BatchSize { get; set; } = batchSize;
    public int WindowDays { get; set; } = windowDays;
    public string Summary { get; set; } = summary;
    public string ContactEmail { get; set; } = contactEmail;
    public double Amount { get; set; } = amount;
    public double Hours { get; set; } = hours;
    public string Segment { get; set; } = segment;
    public string ExternalRef { get; set; } = externalRef;
    public PendingLicenseSeatCheckpointStatus Status { get; set; } = status;

    /// <summary>Starts a pending license seat checkpoint in status requested.</summary>
    public static PendingLicenseSeatCheckpoint NewPendingLicenseSeatCheckpoint(string code, string name)
    {
        return new PendingLicenseSeatCheckpoint(
            code,
            name,
            0,
            0,
            "",
            "",
            0.0,
            0.0,
            "",
            "",
            PendingLicenseSeatCheckpointStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PendingLicenseSeatCheckpointStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PendingLicenseSeatCheckpointStatus next)
    {
        switch (Status)
        {
            case PendingLicenseSeatCheckpointStatus.Requested:
                return next == PendingLicenseSeatCheckpointStatus.Expired;
            case PendingLicenseSeatCheckpointStatus.Granted:
                return next == PendingLicenseSeatCheckpointStatus.Revoked;
            case PendingLicenseSeatCheckpointStatus.Revoked:
                switch (next)
                {
                    case PendingLicenseSeatCheckpointStatus.Expired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(PendingLicenseSeatCheckpointStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"pending license seat checkpoint {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} ({Name})";
    }

    public double AdjustedTotal()
    {
        return Math.Round((double)WindowDays + (double)BatchSize, 2);
    }

    public PendingLicenseSeatCheckpoint Copy()
    {
        return new PendingLicenseSeatCheckpoint(
            Code,
            Name,
            BatchSize,
            WindowDays,
            Summary,
            ContactEmail,
            Amount,
            Hours,
            Segment,
            ExternalRef,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Segment);
    }
}
