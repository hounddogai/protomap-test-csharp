namespace Acme.Workforce.Assets.Refresh.Plan;

using Acme.Workforce.Kit.Text;

/// <summary>Where a refresh plan is in its life cycle.</summary>
public enum RefreshPlanStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>A refresh plan. Its code is unique within a tenant.</summary>
public sealed class RefreshPlan(string code, string label, int expiresAt, int attempts, string segment, bool pinned, double threshold, double fte, RefreshPlanStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int ExpiresAt { get; set; } = expiresAt;
    public int Attempts { get; set; } = attempts;
    public string Segment { get; set; } = segment;
    public bool Pinned { get; set; } = pinned;
    public double Threshold { get; set; } = threshold;
    public double Fte { get; set; } = fte;
    public RefreshPlanStatus Status { get; set; } = status;

    /// <summary>Starts a refresh plan in status planned.</summary>
    public static RefreshPlan NewRefreshPlan(string code, string label)
    {
        return new RefreshPlan(code, label, 0, 0, "", false, 0.0, 0.0, RefreshPlanStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RefreshPlanStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RefreshPlanStatus next)
    {
        switch (Status)
        {
            case RefreshPlanStatus.Planned:
                switch (next)
                {
                    case RefreshPlanStatus.Scheduled:
                    case RefreshPlanStatus.Cancelled:
                        return true;
                }
                return false;
            case RefreshPlanStatus.Scheduled:
                switch (next)
                {
                    case RefreshPlanStatus.Cancelled:
                        return true;
                }
                return false;
            case RefreshPlanStatus.InProgress:
                return next == RefreshPlanStatus.Completed || next == RefreshPlanStatus.Cancelled;
            case RefreshPlanStatus.Completed:
                switch (next)
                {
                    case RefreshPlanStatus.Cancelled:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(RefreshPlanStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"refresh plan {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        ExpiresAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{Code} / {Label}";
    }

    public double AdjustedVolume()
    {
        return Math.Round((double)ExpiresAt - (double)Attempts, 2);
    }

    public RefreshPlan Copy()
    {
        return new RefreshPlan(Code, Label, ExpiresAt, Attempts, Segment, Pinned, Threshold, Fte, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Segment);
    }
}
