namespace Acme.Workforce.Talent.Nomination.NominationPolicy;

using Acme.Workforce.Kit.Text;

/// <summary>Where a scheduled nomination snapshot is in its life cycle.</summary>
public enum ScheduledNominationSnapshotStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>One scheduled nomination snapshot, with what the planners need to act on it.</summary>
public sealed class ScheduledNominationSnapshot(string code, string title, int limit, bool pinned, string locale, ScheduledNominationSnapshotStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int Limit { get; set; } = limit;
    public bool Pinned { get; set; } = pinned;
    public string Locale { get; set; } = locale;
    public ScheduledNominationSnapshotStatus Status { get; set; } = status;

    /// <summary>Starts a scheduled nomination snapshot in status created.</summary>
    public static ScheduledNominationSnapshot NewScheduledNominationSnapshot(string code, string title)
    {
        return new ScheduledNominationSnapshot(code, title, 0, false, "", ScheduledNominationSnapshotStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ScheduledNominationSnapshotStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ScheduledNominationSnapshotStatus next)
    {
        switch (Status)
        {
            case ScheduledNominationSnapshotStatus.Created:
                return next == ScheduledNominationSnapshotStatus.Published || next == ScheduledNominationSnapshotStatus.Retired;
            case ScheduledNominationSnapshotStatus.Validated:
                return next == ScheduledNominationSnapshotStatus.Published || next == ScheduledNominationSnapshotStatus.Retired;
            case ScheduledNominationSnapshotStatus.Published:
                return next == ScheduledNominationSnapshotStatus.Retired;
        }
        return false;
    }

    public void MoveTo(ScheduledNominationSnapshotStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"scheduled nomination snapshot {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} / {Title}";
    }

    public double MeasuredValue()
    {
        return (double)Limit;
    }

    public ScheduledNominationSnapshot Copy()
    {
        return new ScheduledNominationSnapshot(Code, Title, Limit, Pinned, Locale, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Locale);
    }
}
