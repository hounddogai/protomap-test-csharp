namespace Acme.Workforce.Payrollops.RetroPay.Tier;

using Acme.Workforce.Kit.Text;

/// <summary>Where a legacy retro pay snapshot is in its life cycle.</summary>
public enum LegacyRetroPaySnapshotStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>The record of a legacy retro pay snapshot and its current status.</summary>
public sealed class LegacyRetroPaySnapshot(string code, string title, string badge, int createdAt, string checksum, double minimum, int capacity, double amount, LegacyRetroPaySnapshotStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Badge { get; set; } = badge;
    public int CreatedAt { get; set; } = createdAt;
    public string Checksum { get; set; } = checksum;
    public double Minimum { get; set; } = minimum;
    public int Capacity { get; set; } = capacity;
    public double Amount { get; set; } = amount;
    public LegacyRetroPaySnapshotStatus Status { get; set; } = status;

    /// <summary>Starts a legacy retro pay snapshot in status proposed.</summary>
    public static LegacyRetroPaySnapshot NewLegacyRetroPaySnapshot(string code, string title)
    {
        return new LegacyRetroPaySnapshot(
            code,
            title,
            "",
            0,
            "",
            0.0,
            0,
            0.0,
            LegacyRetroPaySnapshotStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LegacyRetroPaySnapshotStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LegacyRetroPaySnapshotStatus next)
    {
        switch (Status)
        {
            case LegacyRetroPaySnapshotStatus.Proposed:
                return next == LegacyRetroPaySnapshotStatus.Approved || next == LegacyRetroPaySnapshotStatus.Rejected;
            case LegacyRetroPaySnapshotStatus.Approved:
                return next == LegacyRetroPaySnapshotStatus.Rejected || next == LegacyRetroPaySnapshotStatus.Withdrawn;
            case LegacyRetroPaySnapshotStatus.Rejected:
                switch (next)
                {
                    case LegacyRetroPaySnapshotStatus.Withdrawn:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(LegacyRetroPaySnapshotStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"legacy retro pay snapshot {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        CreatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} [{Title}]";
    }

    public double AdjustedExposure()
    {
        return Math.Round((double)CreatedAt * (double)Capacity, 2);
    }

    public LegacyRetroPaySnapshot Copy()
    {
        return new LegacyRetroPaySnapshot(
            Code,
            Title,
            Badge,
            CreatedAt,
            Checksum,
            Minimum,
            Capacity,
            Amount,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
