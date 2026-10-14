namespace Acme.Workforce.Absence.Balance.DefaultBalanceGroup;

using Acme.Workforce.Kit.Text;

/// <summary>Where a lenient balance snapshot is in its life cycle.</summary>
public enum LenientBalanceSnapshotStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>One lenient balance snapshot, with what the planners need to act on it.</summary>
public sealed class LenientBalanceSnapshot(string code, string title, int tenureMonths, string source, string name, int startsAt, string externalRef, string label, LenientBalanceSnapshotStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int TenureMonths { get; set; } = tenureMonths;
    public string Source { get; set; } = source;
    public string Name { get; set; } = name;
    public int StartsAt { get; set; } = startsAt;
    public string ExternalRef { get; set; } = externalRef;
    public string Label { get; set; } = label;
    public LenientBalanceSnapshotStatus Status { get; set; } = status;

    /// <summary>Starts a lenient balance snapshot in status proposed.</summary>
    public static LenientBalanceSnapshot NewLenientBalanceSnapshot(string code, string title)
    {
        return new LenientBalanceSnapshot(code, title, 0, "", "", 0, "", "", LenientBalanceSnapshotStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LenientBalanceSnapshotStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LenientBalanceSnapshotStatus next)
    {
        switch (Status)
        {
            case LenientBalanceSnapshotStatus.Proposed:
                switch (next)
                {
                    case LenientBalanceSnapshotStatus.Rejected:
                        return true;
                }
                return false;
            case LenientBalanceSnapshotStatus.Approved:
                return next == LenientBalanceSnapshotStatus.Withdrawn;
            case LenientBalanceSnapshotStatus.Rejected:
                return next == LenientBalanceSnapshotStatus.Withdrawn;
        }
        return false;
    }

    public void MoveTo(LenientBalanceSnapshotStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"lenient balance snapshot {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        StartsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{Code}: {Title}";
    }

    public double ProjectedExposure()
    {
        return Math.Round((double)StartsAt - (double)TenureMonths, 2);
    }

    public LenientBalanceSnapshot Copy()
    {
        return new LenientBalanceSnapshot(
            Code,
            Title,
            TenureMonths,
            Source,
            Name,
            StartsAt,
            ExternalRef,
            Label,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Source);
    }
}
