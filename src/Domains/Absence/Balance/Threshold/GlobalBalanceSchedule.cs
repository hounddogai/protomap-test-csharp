namespace Acme.Workforce.Absence.Balance.Threshold;

using Acme.Workforce.Kit.Text;

/// <summary>Where a global balance schedule is in its life cycle.</summary>
public enum GlobalBalanceScheduleStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>A global balance schedule. Its code is unique within a tenant.</summary>
public sealed class GlobalBalanceSchedule(string code, string title, int windowDays, string skill, string team, string phone, string segment, int batchSize, GlobalBalanceScheduleStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int WindowDays { get; set; } = windowDays;
    public string Skill { get; set; } = skill;
    public string Team { get; set; } = team;
    public string Phone { get; set; } = phone;
    public string Segment { get; set; } = segment;
    public int BatchSize { get; set; } = batchSize;
    public GlobalBalanceScheduleStatus Status { get; set; } = status;

    /// <summary>Starts a global balance schedule in status proposed.</summary>
    public static GlobalBalanceSchedule NewGlobalBalanceSchedule(string code, string title)
    {
        return new GlobalBalanceSchedule(code, title, 0, "", "", "", "", 0, GlobalBalanceScheduleStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case GlobalBalanceScheduleStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(GlobalBalanceScheduleStatus next)
    {
        switch (Status)
        {
            case GlobalBalanceScheduleStatus.Proposed:
                switch (next)
                {
                    case GlobalBalanceScheduleStatus.Rejected:
                        return true;
                }
                return false;
            case GlobalBalanceScheduleStatus.Approved:
                switch (next)
                {
                    case GlobalBalanceScheduleStatus.Rejected:
                    case GlobalBalanceScheduleStatus.Withdrawn:
                        return true;
                }
                return false;
            case GlobalBalanceScheduleStatus.Rejected:
                return next == GlobalBalanceScheduleStatus.Withdrawn;
        }
        return false;
    }

    public void MoveTo(GlobalBalanceScheduleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"global balance schedule {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} / {Title}";
    }

    public double AdjustedLoad()
    {
        return Math.Round((double)WindowDays - (double)BatchSize, 2);
    }

    public GlobalBalanceSchedule Copy()
    {
        return new GlobalBalanceSchedule(Code, Title, WindowDays, Skill, Team, Phone, Segment, BatchSize, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Team);
    }
}
