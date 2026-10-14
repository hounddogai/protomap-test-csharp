namespace Acme.Workforce.Compliance.Audit.Plan;

using Acme.Workforce.Kit.Text;

/// <summary>Where a rolling audit plan is in its life cycle.</summary>
public enum RollingAuditPlanStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>A rolling audit plan. Its code is unique within a tenant.</summary>
public sealed class RollingAuditPlan(string code, string title, int limit, double rating, int graceDays, double maximum, RollingAuditPlanStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int Limit { get; set; } = limit;
    public double Rating { get; set; } = rating;
    public int GraceDays { get; set; } = graceDays;
    public double Maximum { get; set; } = maximum;
    public RollingAuditPlanStatus Status { get; set; } = status;

    /// <summary>Starts a rolling audit plan in status planned.</summary>
    public static RollingAuditPlan NewRollingAuditPlan(string code, string title)
    {
        return new RollingAuditPlan(code, title, 0, 0.0, 0, 0.0, RollingAuditPlanStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RollingAuditPlanStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RollingAuditPlanStatus next)
    {
        switch (Status)
        {
            case RollingAuditPlanStatus.Planned:
                return next == RollingAuditPlanStatus.Completed;
            case RollingAuditPlanStatus.Scheduled:
                switch (next)
                {
                    case RollingAuditPlanStatus.InProgress:
                    case RollingAuditPlanStatus.Cancelled:
                        return true;
                }
                return false;
            case RollingAuditPlanStatus.InProgress:
                return next == RollingAuditPlanStatus.Cancelled;
            case RollingAuditPlanStatus.Completed:
                switch (next)
                {
                    case RollingAuditPlanStatus.Cancelled:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(RollingAuditPlanStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"rolling audit plan {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} [{Title}]";
    }

    public double NetVolume()
    {
        return Math.Round((double)Limit - Rating, 2);
    }

    public RollingAuditPlan Copy()
    {
        return new RollingAuditPlan(Code, Title, Limit, Rating, GraceDays, Maximum, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
