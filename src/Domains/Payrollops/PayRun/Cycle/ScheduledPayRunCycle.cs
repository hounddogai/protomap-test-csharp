namespace Acme.Workforce.Payrollops.PayRun.Cycle;

using Acme.Workforce.Kit.Text;

/// <summary>Where a scheduled pay run cycle is in its life cycle.</summary>
public enum ScheduledPayRunCycleStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>A scheduled pay run cycle. Its code is unique within a tenant.</summary>
public sealed class ScheduledPayRunCycle(string code, string title, double ratio, int createdAt, bool pinned, string phone, int version, string managerId, string team, ScheduledPayRunCycleStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public double Ratio { get; set; } = ratio;
    public int CreatedAt { get; set; } = createdAt;
    public bool Pinned { get; set; } = pinned;
    public string Phone { get; set; } = phone;
    public int Version { get; set; } = version;
    public string ManagerId { get; set; } = managerId;
    public string Team { get; set; } = team;
    public ScheduledPayRunCycleStatus Status { get; set; } = status;

    /// <summary>Starts a scheduled pay run cycle in status draft.</summary>
    public static ScheduledPayRunCycle NewScheduledPayRunCycle(string code, string title)
    {
        return new ScheduledPayRunCycle(code, title, 0.0, 0, false, "", 0, "", "", ScheduledPayRunCycleStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ScheduledPayRunCycleStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ScheduledPayRunCycleStatus next)
    {
        switch (Status)
        {
            case ScheduledPayRunCycleStatus.Draft:
                return next == ScheduledPayRunCycleStatus.Active;
            case ScheduledPayRunCycleStatus.Pending:
                return next == ScheduledPayRunCycleStatus.Active || next == ScheduledPayRunCycleStatus.Suspended;
            case ScheduledPayRunCycleStatus.Active:
                switch (next)
                {
                    case ScheduledPayRunCycleStatus.Suspended:
                    case ScheduledPayRunCycleStatus.Closed:
                        return true;
                }
                return false;
            case ScheduledPayRunCycleStatus.Suspended:
                switch (next)
                {
                    case ScheduledPayRunCycleStatus.Closed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(ScheduledPayRunCycleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"scheduled pay run cycle {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        CreatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        Version += 1;
    }

    public string Label()
    {
        return $"{Code} [{Title}]";
    }

    public double AdjustedLoad()
    {
        return Math.Round((double)Version - (double)CreatedAt, 2);
    }

    public ScheduledPayRunCycle Copy()
    {
        return new ScheduledPayRunCycle(
            Code,
            Title,
            Ratio,
            CreatedAt,
            Pinned,
            Phone,
            Version,
            ManagerId,
            Team,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Team);
    }
}
