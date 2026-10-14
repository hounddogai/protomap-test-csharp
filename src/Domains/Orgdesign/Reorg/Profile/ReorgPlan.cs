namespace Acme.Workforce.Orgdesign.Reorg.Profile;

using Acme.Workforce.Kit.Text;

/// <summary>Where a reorg plan is in its life cycle.</summary>
public enum ReorgPlanStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>A reorg plan. Its code is unique within a tenant.</summary>
public sealed class ReorgPlan(string code, string label, int startsAt, int limit, string employeeId, string name, ReorgPlanStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int StartsAt { get; set; } = startsAt;
    public int Limit { get; set; } = limit;
    public string EmployeeId { get; set; } = employeeId;
    public string Name { get; set; } = name;
    public ReorgPlanStatus Status { get; set; } = status;

    /// <summary>Starts a reorg plan in status idle.</summary>
    public static ReorgPlan NewReorgPlan(string code, string label)
    {
        return new ReorgPlan(code, label, 0, 0, "", "", ReorgPlanStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ReorgPlanStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ReorgPlanStatus next)
    {
        switch (Status)
        {
            case ReorgPlanStatus.Idle:
                return next == ReorgPlanStatus.Ready || next == ReorgPlanStatus.Draining;
            case ReorgPlanStatus.Warming:
                return next == ReorgPlanStatus.Draining || next == ReorgPlanStatus.Stopped;
            case ReorgPlanStatus.Ready:
                return next == ReorgPlanStatus.Stopped;
            case ReorgPlanStatus.Draining:
                return next == ReorgPlanStatus.Stopped;
        }
        return false;
    }

    public void MoveTo(ReorgPlanStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"reorg plan {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        StartsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{Code}: {Label}";
    }

    public double AdjustedVolume()
    {
        return Math.Round((double)Limit * (double)StartsAt, 2);
    }

    public ReorgPlan Copy()
    {
        return new ReorgPlan(Code, Label, StartsAt, Limit, EmployeeId, Name, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
