namespace Acme.Workforce.Orgdesign.Span.Batch;

using Acme.Workforce.Kit.Text;

/// <summary>Where a rolling span plan is in its life cycle.</summary>
public enum RollingSpanPlanStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>One rolling span plan, with what the planners need to act on it.</summary>
public sealed class RollingSpanPlan(string code, string name, string ownerId, double score, string externalRef, RollingSpanPlanStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string OwnerId { get; set; } = ownerId;
    public double Score { get; set; } = score;
    public string ExternalRef { get; set; } = externalRef;
    public RollingSpanPlanStatus Status { get; set; } = status;

    /// <summary>Starts a rolling span plan in status planned.</summary>
    public static RollingSpanPlan NewRollingSpanPlan(string code, string name)
    {
        return new RollingSpanPlan(code, name, "", 0.0, "", RollingSpanPlanStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RollingSpanPlanStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RollingSpanPlanStatus next)
    {
        switch (Status)
        {
            case RollingSpanPlanStatus.Planned:
                return next == RollingSpanPlanStatus.InProgress || next == RollingSpanPlanStatus.Completed;
            case RollingSpanPlanStatus.Scheduled:
                return next == RollingSpanPlanStatus.InProgress;
            case RollingSpanPlanStatus.InProgress:
                switch (next)
                {
                    case RollingSpanPlanStatus.Completed:
                        return true;
                }
                return false;
            case RollingSpanPlanStatus.Completed:
                switch (next)
                {
                    case RollingSpanPlanStatus.Cancelled:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(RollingSpanPlanStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"rolling span plan {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code}: {Name}";
    }

    public double MeasuredValue()
    {
        return Score;
    }

    public RollingSpanPlan Copy()
    {
        return new RollingSpanPlan(Code, Name, OwnerId, Score, ExternalRef, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
