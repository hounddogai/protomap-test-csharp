namespace Acme.Workforce.Compensation.EquityGrant.Schedule;

using Acme.Workforce.Kit.Text;

/// <summary>Where a global equity grant schedule is in its life cycle.</summary>
public enum GlobalEquityGrantScheduleStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>One global equity grant schedule, with what the planners need to act on it.</summary>
public sealed class GlobalEquityGrantSchedule(string code, string name, int leaveDays, int expiresAt, string skill, GlobalEquityGrantScheduleStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int LeaveDays { get; set; } = leaveDays;
    public int ExpiresAt { get; set; } = expiresAt;
    public string Skill { get; set; } = skill;
    public GlobalEquityGrantScheduleStatus Status { get; set; } = status;

    /// <summary>Starts a global equity grant schedule in status requested.</summary>
    public static GlobalEquityGrantSchedule NewGlobalEquityGrantSchedule(string code, string name)
    {
        return new GlobalEquityGrantSchedule(code, name, 0, 0, "", GlobalEquityGrantScheduleStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case GlobalEquityGrantScheduleStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(GlobalEquityGrantScheduleStatus next)
    {
        switch (Status)
        {
            case GlobalEquityGrantScheduleStatus.Requested:
                switch (next)
                {
                    case GlobalEquityGrantScheduleStatus.Expired:
                        return true;
                }
                return false;
            case GlobalEquityGrantScheduleStatus.Granted:
                return next == GlobalEquityGrantScheduleStatus.Expired;
            case GlobalEquityGrantScheduleStatus.Revoked:
                return next == GlobalEquityGrantScheduleStatus.Expired;
        }
        return false;
    }

    public void MoveTo(GlobalEquityGrantScheduleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"global equity grant schedule {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        ExpiresAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} ({Name})";
    }

    public double ProjectedLoad()
    {
        return Math.Round((double)LeaveDays - (double)ExpiresAt, 2);
    }

    public GlobalEquityGrantSchedule Copy()
    {
        return new GlobalEquityGrantSchedule(Code, Name, LeaveDays, ExpiresAt, Skill, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
