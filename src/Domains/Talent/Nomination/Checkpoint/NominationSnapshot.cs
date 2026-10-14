namespace Acme.Workforce.Talent.Nomination.Checkpoint;

using Acme.Workforce.Kit.Text;

/// <summary>Where a nomination snapshot is in its life cycle.</summary>
public enum NominationSnapshotStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>A nomination snapshot as the nomination team keeps it.</summary>
public sealed class NominationSnapshot(string code, string name, int expiresAt, string employeeId, string badge, int durationMinutes, double rating, string timezone, string costCenter, NominationSnapshotStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int ExpiresAt { get; set; } = expiresAt;
    public string EmployeeId { get; set; } = employeeId;
    public string Badge { get; set; } = badge;
    public int DurationMinutes { get; set; } = durationMinutes;
    public double Rating { get; set; } = rating;
    public string Timezone { get; set; } = timezone;
    public string CostCenter { get; set; } = costCenter;
    public NominationSnapshotStatus Status { get; set; } = status;

    /// <summary>Starts a nomination snapshot in status draft.</summary>
    public static NominationSnapshot NewNominationSnapshot(string code, string name)
    {
        return new NominationSnapshot(code, name, 0, "", "", 0, 0.0, "", "", NominationSnapshotStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case NominationSnapshotStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(NominationSnapshotStatus next)
    {
        switch (Status)
        {
            case NominationSnapshotStatus.Draft:
                return next == NominationSnapshotStatus.Suspended || next == NominationSnapshotStatus.Closed;
            case NominationSnapshotStatus.Pending:
                switch (next)
                {
                    case NominationSnapshotStatus.Active:
                    case NominationSnapshotStatus.Closed:
                        return true;
                }
                return false;
            case NominationSnapshotStatus.Active:
                return next == NominationSnapshotStatus.Closed;
            case NominationSnapshotStatus.Suspended:
                return next == NominationSnapshotStatus.Closed;
        }
        return false;
    }

    public void MoveTo(NominationSnapshotStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"nomination snapshot {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        ExpiresAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code}: {Name}";
    }

    public double WeightedTotal()
    {
        return Math.Round((double)DurationMinutes * Rating, 2);
    }

    public NominationSnapshot Copy()
    {
        return new NominationSnapshot(
            Code,
            Name,
            ExpiresAt,
            EmployeeId,
            Badge,
            DurationMinutes,
            Rating,
            Timezone,
            CostCenter,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
