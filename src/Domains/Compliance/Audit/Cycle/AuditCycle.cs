namespace Acme.Workforce.Compliance.Audit.Cycle;

using Acme.Workforce.Kit.Text;

/// <summary>Where a audit cycle is in its life cycle.</summary>
public enum AuditCycleStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>A audit cycle. Its code is unique within a tenant.</summary>
public sealed class AuditCycle(string code, string label, double budget, string ownerId, bool archived, double minimum, bool visible, int tenureMonths, string unit, int updatedAt, AuditCycleStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public double Budget { get; set; } = budget;
    public string OwnerId { get; set; } = ownerId;
    public bool Archived { get; set; } = archived;
    public double Minimum { get; set; } = minimum;
    public bool Visible { get; set; } = visible;
    public int TenureMonths { get; set; } = tenureMonths;
    public string Unit { get; set; } = unit;
    public int UpdatedAt { get; set; } = updatedAt;
    public AuditCycleStatus Status { get; set; } = status;

    /// <summary>Starts a audit cycle in status draft.</summary>
    public static AuditCycle NewAuditCycle(string code, string label)
    {
        return new AuditCycle(code, label, 0.0, "", false, 0.0, false, 0, "", 0, AuditCycleStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AuditCycleStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AuditCycleStatus next)
    {
        switch (Status)
        {
            case AuditCycleStatus.Draft:
                return next == AuditCycleStatus.Pending || next == AuditCycleStatus.Active;
            case AuditCycleStatus.Pending:
                switch (next)
                {
                    case AuditCycleStatus.Suspended:
                    case AuditCycleStatus.Closed:
                        return true;
                }
                return false;
            case AuditCycleStatus.Active:
                switch (next)
                {
                    case AuditCycleStatus.Suspended:
                        return true;
                }
                return false;
            case AuditCycleStatus.Suspended:
                switch (next)
                {
                    case AuditCycleStatus.Closed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(AuditCycleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"audit cycle {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        UpdatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{Code} ({Label})";
    }

    public double AdjustedTotal()
    {
        return Math.Round((double)TenureMonths - Budget, 2);
    }

    public AuditCycle Copy()
    {
        return new AuditCycle(
            Code,
            Label,
            Budget,
            OwnerId,
            Archived,
            Minimum,
            Visible,
            TenureMonths,
            Unit,
            UpdatedAt,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Unit);
    }
}
