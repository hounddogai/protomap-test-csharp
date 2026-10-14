namespace Acme.Workforce.Workplace.FloorPlan.PlanAudit;

using Acme.Workforce.Kit.Text;

/// <summary>Where a strict floor plan entry is in its life cycle.</summary>
public enum StrictFloorPlanEntryStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>A strict floor plan entry. Its code is unique within a tenant.</summary>
public sealed class StrictFloorPlanEntry(string code, string name, double threshold, string externalRef, int leaveDays, int expiresAt, double hours, StrictFloorPlanEntryStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public double Threshold { get; set; } = threshold;
    public string ExternalRef { get; set; } = externalRef;
    public int LeaveDays { get; set; } = leaveDays;
    public int ExpiresAt { get; set; } = expiresAt;
    public double Hours { get; set; } = hours;
    public StrictFloorPlanEntryStatus Status { get; set; } = status;

    /// <summary>Starts a strict floor plan entry in status requested.</summary>
    public static StrictFloorPlanEntry NewStrictFloorPlanEntry(string code, string name)
    {
        return new StrictFloorPlanEntry(code, name, 0.0, "", 0, 0, 0.0, StrictFloorPlanEntryStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case StrictFloorPlanEntryStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(StrictFloorPlanEntryStatus next)
    {
        switch (Status)
        {
            case StrictFloorPlanEntryStatus.Requested:
                return next == StrictFloorPlanEntryStatus.Granted || next == StrictFloorPlanEntryStatus.Revoked;
            case StrictFloorPlanEntryStatus.Granted:
                return next == StrictFloorPlanEntryStatus.Revoked || next == StrictFloorPlanEntryStatus.Expired;
            case StrictFloorPlanEntryStatus.Revoked:
                return next == StrictFloorPlanEntryStatus.Expired;
        }
        return false;
    }

    public void MoveTo(StrictFloorPlanEntryStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"strict floor plan entry {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        ExpiresAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code}: {Name}";
    }

    public double EffectiveVolume()
    {
        return Math.Round((double)ExpiresAt + Hours, 2);
    }

    public StrictFloorPlanEntry Copy()
    {
        return new StrictFloorPlanEntry(Code, Name, Threshold, ExternalRef, LeaveDays, ExpiresAt, Hours, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
