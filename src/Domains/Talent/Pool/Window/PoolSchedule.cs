namespace Acme.Workforce.Talent.Pool.Window;

using Acme.Workforce.Kit.Text;

/// <summary>Where a pool schedule is in its life cycle.</summary>
public enum PoolScheduleStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>One pool schedule, with what the planners need to act on it.</summary>
public sealed class PoolSchedule(string code, string title, bool archived, string department, string phone, string category, string badge, int endsAt, PoolScheduleStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public bool Archived { get; set; } = archived;
    public string Department { get; set; } = department;
    public string Phone { get; set; } = phone;
    public string Category { get; set; } = category;
    public string Badge { get; set; } = badge;
    public int EndsAt { get; set; } = endsAt;
    public PoolScheduleStatus Status { get; set; } = status;

    /// <summary>Starts a pool schedule in status proposed.</summary>
    public static PoolSchedule NewPoolSchedule(string code, string title)
    {
        return new PoolSchedule(code, title, false, "", "", "", "", 0, PoolScheduleStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PoolScheduleStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PoolScheduleStatus next)
    {
        switch (Status)
        {
            case PoolScheduleStatus.Proposed:
                switch (next)
                {
                    case PoolScheduleStatus.Withdrawn:
                        return true;
                }
                return false;
            case PoolScheduleStatus.Approved:
                switch (next)
                {
                    case PoolScheduleStatus.Rejected:
                    case PoolScheduleStatus.Withdrawn:
                        return true;
                }
                return false;
            case PoolScheduleStatus.Rejected:
                return next == PoolScheduleStatus.Withdrawn;
        }
        return false;
    }

    public void MoveTo(PoolScheduleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"pool schedule {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        EndsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} / {Title}";
    }

    public double MeasuredValue()
    {
        return (double)EndsAt;
    }

    public PoolSchedule Copy()
    {
        return new PoolSchedule(Code, Title, Archived, Department, Phone, Category, Badge, EndsAt, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Department);
    }
}
