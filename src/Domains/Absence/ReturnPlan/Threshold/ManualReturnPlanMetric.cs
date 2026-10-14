namespace Acme.Workforce.Absence.ReturnPlan.Threshold;

using Acme.Workforce.Kit.Text;

/// <summary>Where a manual return plan metric is in its life cycle.</summary>
public enum ManualReturnPlanMetricStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>A manual return plan metric as the return plan team keeps it.</summary>
public sealed class ManualReturnPlanMetric(string code, string name, string segment, int expiresAt, string ownerId, double fte, double discount, ManualReturnPlanMetricStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Segment { get; set; } = segment;
    public int ExpiresAt { get; set; } = expiresAt;
    public string OwnerId { get; set; } = ownerId;
    public double Fte { get; set; } = fte;
    public double Discount { get; set; } = discount;
    public ManualReturnPlanMetricStatus Status { get; set; } = status;

    /// <summary>Starts a manual return plan metric in status open.</summary>
    public static ManualReturnPlanMetric NewManualReturnPlanMetric(string code, string name)
    {
        return new ManualReturnPlanMetric(code, name, "", 0, "", 0.0, 0.0, ManualReturnPlanMetricStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ManualReturnPlanMetricStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ManualReturnPlanMetricStatus next)
    {
        switch (Status)
        {
            case ManualReturnPlanMetricStatus.Open:
                switch (next)
                {
                    case ManualReturnPlanMetricStatus.Archived:
                        return true;
                }
                return false;
            case ManualReturnPlanMetricStatus.InReview:
                switch (next)
                {
                    case ManualReturnPlanMetricStatus.Reopened:
                        return true;
                }
                return false;
            case ManualReturnPlanMetricStatus.Resolved:
                return next == ManualReturnPlanMetricStatus.Reopened;
            case ManualReturnPlanMetricStatus.Reopened:
                switch (next)
                {
                    case ManualReturnPlanMetricStatus.Archived:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(ManualReturnPlanMetricStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"manual return plan metric {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        ExpiresAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code}: {Name}";
    }

    public double AdjustedLoad()
    {
        return Math.Round(Discount * (double)ExpiresAt, 2);
    }

    public ManualReturnPlanMetric Copy()
    {
        return new ManualReturnPlanMetric(Code, Name, Segment, ExpiresAt, OwnerId, Fte, Discount, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Segment);
    }
}
