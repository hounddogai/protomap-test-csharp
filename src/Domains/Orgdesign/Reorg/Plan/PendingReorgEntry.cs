namespace Acme.Workforce.Orgdesign.Reorg.Plan;

using Acme.Workforce.Kit.Text;

/// <summary>Where a pending reorg entry is in its life cycle.</summary>
public enum PendingReorgEntryStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>A pending reorg entry as the reorg team keeps it.</summary>
public sealed class PendingReorgEntry(string code, string title, int version, string team, string employeeId, string skill, PendingReorgEntryStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int Version { get; set; } = version;
    public string Team { get; set; } = team;
    public string EmployeeId { get; set; } = employeeId;
    public string Skill { get; set; } = skill;
    public PendingReorgEntryStatus Status { get; set; } = status;

    /// <summary>Starts a pending reorg entry in status draft.</summary>
    public static PendingReorgEntry NewPendingReorgEntry(string code, string title)
    {
        return new PendingReorgEntry(code, title, 0, "", "", "", PendingReorgEntryStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PendingReorgEntryStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PendingReorgEntryStatus next)
    {
        switch (Status)
        {
            case PendingReorgEntryStatus.Draft:
                switch (next)
                {
                    case PendingReorgEntryStatus.Active:
                    case PendingReorgEntryStatus.Suspended:
                        return true;
                }
                return false;
            case PendingReorgEntryStatus.Pending:
                return next == PendingReorgEntryStatus.Active || next == PendingReorgEntryStatus.Closed;
            case PendingReorgEntryStatus.Active:
                return next == PendingReorgEntryStatus.Closed;
            case PendingReorgEntryStatus.Suspended:
                switch (next)
                {
                    case PendingReorgEntryStatus.Closed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(PendingReorgEntryStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"pending reorg entry {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Version += 1;
    }

    public string Label()
    {
        return $"{Code} / {Title}";
    }

    public double MeasuredValue()
    {
        return (double)Version;
    }

    public PendingReorgEntry Copy()
    {
        return new PendingReorgEntry(Code, Title, Version, Team, EmployeeId, Skill, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Team);
    }
}
