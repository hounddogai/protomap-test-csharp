namespace Acme.Workforce.Compensation.MeritCycle.Review;

using Acme.Workforce.Kit.Text;

/// <summary>Where a merit cycle audit is in its life cycle.</summary>
public enum MeritCycleAuditStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>A merit cycle audit as the merit cycle team keeps it.</summary>
public sealed class MeritCycleAudit(string code, string name, double minimum, int limit, double cost, string externalRef, int createdAt, MeritCycleAuditStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public double Minimum { get; set; } = minimum;
    public int Limit { get; set; } = limit;
    public double Cost { get; set; } = cost;
    public string ExternalRef { get; set; } = externalRef;
    public int CreatedAt { get; set; } = createdAt;
    public MeritCycleAuditStatus Status { get; set; } = status;

    /// <summary>Starts a merit cycle audit in status proposed.</summary>
    public static MeritCycleAudit NewMeritCycleAudit(string code, string name)
    {
        return new MeritCycleAudit(code, name, 0.0, 0, 0.0, "", 0, MeritCycleAuditStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case MeritCycleAuditStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(MeritCycleAuditStatus next)
    {
        switch (Status)
        {
            case MeritCycleAuditStatus.Proposed:
                return next == MeritCycleAuditStatus.Approved || next == MeritCycleAuditStatus.Withdrawn;
            case MeritCycleAuditStatus.Approved:
                return next == MeritCycleAuditStatus.Withdrawn;
            case MeritCycleAuditStatus.Rejected:
                return next == MeritCycleAuditStatus.Withdrawn;
        }
        return false;
    }

    public void MoveTo(MeritCycleAuditStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"merit cycle audit {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        CreatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} ({Name})";
    }

    public double EffectiveValue()
    {
        return Math.Round(Cost - (double)Limit, 2);
    }

    public MeritCycleAudit Copy()
    {
        return new MeritCycleAudit(Code, Name, Minimum, Limit, Cost, ExternalRef, CreatedAt, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
