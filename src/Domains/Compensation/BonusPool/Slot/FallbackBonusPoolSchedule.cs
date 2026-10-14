namespace Acme.Workforce.Compensation.BonusPool.Slot;

using Acme.Workforce.Kit.Text;

/// <summary>Where a fallback bonus pool schedule is in its life cycle.</summary>
public enum FallbackBonusPoolScheduleStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>The record of a fallback bonus pool schedule and its current status.</summary>
public sealed class FallbackBonusPoolSchedule(string code, string name, int version, string employeeId, string badge, bool approved, int endsAt, FallbackBonusPoolScheduleStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int Version { get; set; } = version;
    public string EmployeeId { get; set; } = employeeId;
    public string Badge { get; set; } = badge;
    public bool Approved { get; set; } = approved;
    public int EndsAt { get; set; } = endsAt;
    public FallbackBonusPoolScheduleStatus Status { get; set; } = status;

    /// <summary>Starts a fallback bonus pool schedule in status draft.</summary>
    public static FallbackBonusPoolSchedule NewFallbackBonusPoolSchedule(string code, string name)
    {
        return new FallbackBonusPoolSchedule(code, name, 0, "", "", false, 0, FallbackBonusPoolScheduleStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case FallbackBonusPoolScheduleStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(FallbackBonusPoolScheduleStatus next)
    {
        switch (Status)
        {
            case FallbackBonusPoolScheduleStatus.Draft:
                return next == FallbackBonusPoolScheduleStatus.Pending || next == FallbackBonusPoolScheduleStatus.Suspended;
            case FallbackBonusPoolScheduleStatus.Pending:
                switch (next)
                {
                    case FallbackBonusPoolScheduleStatus.Active:
                    case FallbackBonusPoolScheduleStatus.Suspended:
                        return true;
                }
                return false;
            case FallbackBonusPoolScheduleStatus.Active:
                switch (next)
                {
                    case FallbackBonusPoolScheduleStatus.Closed:
                        return true;
                }
                return false;
            case FallbackBonusPoolScheduleStatus.Suspended:
                return next == FallbackBonusPoolScheduleStatus.Closed;
        }
        return false;
    }

    public void MoveTo(FallbackBonusPoolScheduleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"fallback bonus pool schedule {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        EndsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        Version += 1;
    }

    public string Label()
    {
        return $"{Code} / {Name}";
    }

    public double ProjectedTotal()
    {
        return Math.Round((double)EndsAt - (double)Version, 2);
    }

    public FallbackBonusPoolSchedule Copy()
    {
        return new FallbackBonusPoolSchedule(Code, Name, Version, EmployeeId, Badge, Approved, EndsAt, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
