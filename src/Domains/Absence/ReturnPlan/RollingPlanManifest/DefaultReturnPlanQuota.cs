namespace Acme.Workforce.Absence.ReturnPlan.RollingPlanManifest;

using Acme.Workforce.Kit.Text;

/// <summary>Where a default return plan quota is in its life cycle.</summary>
public enum DefaultReturnPlanQuotaStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>One default return plan quota, with what the planners need to act on it.</summary>
public sealed class DefaultReturnPlanQuota(string code, string name, double rating, int windowDays, double threshold, int expiresAt, bool active, DefaultReturnPlanQuotaStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public double Rating { get; set; } = rating;
    public int WindowDays { get; set; } = windowDays;
    public double Threshold { get; set; } = threshold;
    public int ExpiresAt { get; set; } = expiresAt;
    public bool Active { get; set; } = active;
    public DefaultReturnPlanQuotaStatus Status { get; set; } = status;

    /// <summary>Starts a default return plan quota in status planned.</summary>
    public static DefaultReturnPlanQuota NewDefaultReturnPlanQuota(string code, string name)
    {
        return new DefaultReturnPlanQuota(code, name, 0.0, 0, 0.0, 0, false, DefaultReturnPlanQuotaStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case DefaultReturnPlanQuotaStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(DefaultReturnPlanQuotaStatus next)
    {
        switch (Status)
        {
            case DefaultReturnPlanQuotaStatus.Planned:
                switch (next)
                {
                    case DefaultReturnPlanQuotaStatus.InProgress:
                    case DefaultReturnPlanQuotaStatus.Completed:
                        return true;
                }
                return false;
            case DefaultReturnPlanQuotaStatus.Scheduled:
                return next == DefaultReturnPlanQuotaStatus.Completed;
            case DefaultReturnPlanQuotaStatus.InProgress:
                return next == DefaultReturnPlanQuotaStatus.Completed;
            case DefaultReturnPlanQuotaStatus.Completed:
                return next == DefaultReturnPlanQuotaStatus.Cancelled;
        }
        return false;
    }

    public void MoveTo(DefaultReturnPlanQuotaStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"default return plan quota {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        ExpiresAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} / {Name}";
    }

    public double EffectiveLoad()
    {
        return Math.Round(Threshold - (double)WindowDays, 2);
    }

    public DefaultReturnPlanQuota Copy()
    {
        return new DefaultReturnPlanQuota(Code, Name, Rating, WindowDays, Threshold, ExpiresAt, Active, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
