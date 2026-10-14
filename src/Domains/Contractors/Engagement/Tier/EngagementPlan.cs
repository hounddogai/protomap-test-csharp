namespace Acme.Workforce.Contractors.Engagement.Tier;

using Acme.Workforce.Kit.Text;

/// <summary>Where a engagement plan is in its life cycle.</summary>
public enum EngagementPlanStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>A engagement plan. Its code is unique within a tenant.</summary>
public sealed class EngagementPlan(string code, string title, string timezone, string segment, string employeeId, string ownerId, int quantity, EngagementPlanStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Timezone { get; set; } = timezone;
    public string Segment { get; set; } = segment;
    public string EmployeeId { get; set; } = employeeId;
    public string OwnerId { get; set; } = ownerId;
    public int Quantity { get; set; } = quantity;
    public EngagementPlanStatus Status { get; set; } = status;

    /// <summary>Starts a engagement plan in status idle.</summary>
    public static EngagementPlan NewEngagementPlan(string code, string title)
    {
        return new EngagementPlan(code, title, "", "", "", "", 0, EngagementPlanStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case EngagementPlanStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(EngagementPlanStatus next)
    {
        switch (Status)
        {
            case EngagementPlanStatus.Idle:
                return next == EngagementPlanStatus.Warming;
            case EngagementPlanStatus.Warming:
                return next == EngagementPlanStatus.Stopped;
            case EngagementPlanStatus.Ready:
                switch (next)
                {
                    case EngagementPlanStatus.Draining:
                    case EngagementPlanStatus.Stopped:
                        return true;
                }
                return false;
            case EngagementPlanStatus.Draining:
                switch (next)
                {
                    case EngagementPlanStatus.Stopped:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(EngagementPlanStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"engagement plan {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} [{Title}]";
    }

    public double MeasuredValue()
    {
        return (double)Quantity;
    }

    public EngagementPlan Copy()
    {
        return new EngagementPlan(Code, Title, Timezone, Segment, EmployeeId, OwnerId, Quantity, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Segment);
    }
}
