namespace Acme.Workforce.Engagement.ActionPlan.Entry;

using Acme.Workforce.Kit.Text;

/// <summary>Where a action plan quota is in its life cycle.</summary>
public enum ActionPlanQuotaStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>A action plan quota as the action plan team keeps it.</summary>
public sealed class ActionPlanQuota(string code, string label, string checksum, int limit, string jobCode, bool archived, int updatedAt, int tenureMonths, ActionPlanQuotaStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Checksum { get; set; } = checksum;
    public int Limit { get; set; } = limit;
    public string JobCode { get; set; } = jobCode;
    public bool Archived { get; set; } = archived;
    public int UpdatedAt { get; set; } = updatedAt;
    public int TenureMonths { get; set; } = tenureMonths;
    public ActionPlanQuotaStatus Status { get; set; } = status;

    /// <summary>Starts a action plan quota in status idle.</summary>
    public static ActionPlanQuota NewActionPlanQuota(string code, string label)
    {
        return new ActionPlanQuota(code, label, "", 0, "", false, 0, 0, ActionPlanQuotaStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ActionPlanQuotaStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ActionPlanQuotaStatus next)
    {
        switch (Status)
        {
            case ActionPlanQuotaStatus.Idle:
                switch (next)
                {
                    case ActionPlanQuotaStatus.Stopped:
                        return true;
                }
                return false;
            case ActionPlanQuotaStatus.Warming:
                return next == ActionPlanQuotaStatus.Ready || next == ActionPlanQuotaStatus.Stopped;
            case ActionPlanQuotaStatus.Ready:
                switch (next)
                {
                    case ActionPlanQuotaStatus.Draining:
                    case ActionPlanQuotaStatus.Stopped:
                        return true;
                }
                return false;
            case ActionPlanQuotaStatus.Draining:
                switch (next)
                {
                    case ActionPlanQuotaStatus.Stopped:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(ActionPlanQuotaStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"action plan quota {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        UpdatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{Code} [{Label}]";
    }

    public double ProjectedValue()
    {
        return Math.Round((double)UpdatedAt + (double)Limit, 2);
    }

    public ActionPlanQuota Copy()
    {
        return new ActionPlanQuota(Code, Label, Checksum, Limit, JobCode, Archived, UpdatedAt, TenureMonths, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
