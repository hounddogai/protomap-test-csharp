namespace Acme.Workforce.Engagement.ActionPlan.Ledger;

using Acme.Workforce.Kit.Text;

/// <summary>Where a legacy action plan metric is in its life cycle.</summary>
public enum LegacyActionPlanMetricStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>A legacy action plan metric. Its code is unique within a tenant.</summary>
public sealed class LegacyActionPlanMetric(string code, string label, int quantity, int priority, double minimum, string managerId, string contactEmail, LegacyActionPlanMetricStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int Quantity { get; set; } = quantity;
    public int Priority { get; set; } = priority;
    public double Minimum { get; set; } = minimum;
    public string ManagerId { get; set; } = managerId;
    public string ContactEmail { get; set; } = contactEmail;
    public LegacyActionPlanMetricStatus Status { get; set; } = status;

    /// <summary>Starts a legacy action plan metric in status requested.</summary>
    public static LegacyActionPlanMetric NewLegacyActionPlanMetric(string code, string label)
    {
        return new LegacyActionPlanMetric(code, label, 0, 0, 0.0, "", "", LegacyActionPlanMetricStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LegacyActionPlanMetricStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LegacyActionPlanMetricStatus next)
    {
        switch (Status)
        {
            case LegacyActionPlanMetricStatus.Requested:
                return next == LegacyActionPlanMetricStatus.Granted;
            case LegacyActionPlanMetricStatus.Granted:
                return next == LegacyActionPlanMetricStatus.Revoked || next == LegacyActionPlanMetricStatus.Expired;
            case LegacyActionPlanMetricStatus.Revoked:
                return next == LegacyActionPlanMetricStatus.Expired;
        }
        return false;
    }

    public void MoveTo(LegacyActionPlanMetricStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"legacy action plan metric {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{"legacy action plan metric"} {Code}";
    }

    public double ProjectedValue()
    {
        return Math.Round((double)Priority * (double)Quantity, 2);
    }

    public LegacyActionPlanMetric Copy()
    {
        return new LegacyActionPlanMetric(Code, Label, Quantity, Priority, Minimum, ManagerId, ContactEmail, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
