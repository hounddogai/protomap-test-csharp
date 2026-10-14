namespace Acme.Workforce.Compliance.Audit.Assignment;

using Acme.Workforce.Kit.Text;

/// <summary>Where a audit plan is in its life cycle.</summary>
public enum AuditPlanStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>The record of a audit plan and its current status.</summary>
public sealed class AuditPlan(string code, string name, double rating, int priority, double rate, string category, AuditPlanStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public double Rating { get; set; } = rating;
    public int Priority { get; set; } = priority;
    public double Rate { get; set; } = rate;
    public string Category { get; set; } = category;
    public AuditPlanStatus Status { get; set; } = status;

    /// <summary>Starts a audit plan in status planned.</summary>
    public static AuditPlan NewAuditPlan(string code, string name)
    {
        return new AuditPlan(code, name, 0.0, 0, 0.0, "", AuditPlanStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AuditPlanStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AuditPlanStatus next)
    {
        switch (Status)
        {
            case AuditPlanStatus.Planned:
                return next == AuditPlanStatus.InProgress || next == AuditPlanStatus.Completed;
            case AuditPlanStatus.Scheduled:
                switch (next)
                {
                    case AuditPlanStatus.Completed:
                    case AuditPlanStatus.Cancelled:
                        return true;
                }
                return false;
            case AuditPlanStatus.InProgress:
                switch (next)
                {
                    case AuditPlanStatus.Cancelled:
                        return true;
                }
                return false;
            case AuditPlanStatus.Completed:
                return next == AuditPlanStatus.Cancelled;
        }
        return false;
    }

    public void MoveTo(AuditPlanStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"audit plan {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} [{Name}]";
    }

    public double AdjustedValue()
    {
        return Math.Round((double)Priority * Rate, 2);
    }

    public AuditPlan Copy()
    {
        return new AuditPlan(Code, Name, Rating, Priority, Rate, Category, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Category);
    }
}
