namespace Acme.Workforce.Contractors.Rate.Snapshot;

using Acme.Workforce.Kit.Text;

/// <summary>Where a seasonal rate snapshot is in its life cycle.</summary>
public enum SeasonalRateSnapshotStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>A seasonal rate snapshot. Its code is unique within a tenant.</summary>
public sealed class SeasonalRateSnapshot(string code, string label, int attempts, int updatedAt, bool visible, string channel, double hours, SeasonalRateSnapshotStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int Attempts { get; set; } = attempts;
    public int UpdatedAt { get; set; } = updatedAt;
    public bool Visible { get; set; } = visible;
    public string Channel { get; set; } = channel;
    public double Hours { get; set; } = hours;
    public SeasonalRateSnapshotStatus Status { get; set; } = status;

    /// <summary>Starts a seasonal rate snapshot in status created.</summary>
    public static SeasonalRateSnapshot NewSeasonalRateSnapshot(string code, string label)
    {
        return new SeasonalRateSnapshot(code, label, 0, 0, false, "", 0.0, SeasonalRateSnapshotStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case SeasonalRateSnapshotStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(SeasonalRateSnapshotStatus next)
    {
        switch (Status)
        {
            case SeasonalRateSnapshotStatus.Created:
                return next == SeasonalRateSnapshotStatus.Published;
            case SeasonalRateSnapshotStatus.Validated:
                return next == SeasonalRateSnapshotStatus.Retired;
            case SeasonalRateSnapshotStatus.Published:
                return next == SeasonalRateSnapshotStatus.Retired;
        }
        return false;
    }

    public void MoveTo(SeasonalRateSnapshotStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"seasonal rate snapshot {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        UpdatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{Code} [{Label}]";
    }

    public double AdjustedLoad()
    {
        return Math.Round((double)Attempts * Hours, 2);
    }

    public SeasonalRateSnapshot Copy()
    {
        return new SeasonalRateSnapshot(Code, Label, Attempts, UpdatedAt, Visible, Channel, Hours, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Channel);
    }
}
