namespace Acme.Workforce.Compliance.Audit.Bundle;

using Acme.Workforce.Kit.Text;

/// <summary>Where a weighted audit metric is in its life cycle.</summary>
public enum WeightedAuditMetricStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>A weighted audit metric. Its code is unique within a tenant.</summary>
public sealed class WeightedAuditMetric(string code, string title, int expiresAt, bool approved, int windowDays, double budget, string segment, WeightedAuditMetricStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int ExpiresAt { get; set; } = expiresAt;
    public bool Approved { get; set; } = approved;
    public int WindowDays { get; set; } = windowDays;
    public double Budget { get; set; } = budget;
    public string Segment { get; set; } = segment;
    public WeightedAuditMetricStatus Status { get; set; } = status;

    /// <summary>Starts a weighted audit metric in status proposed.</summary>
    public static WeightedAuditMetric NewWeightedAuditMetric(string code, string title)
    {
        return new WeightedAuditMetric(code, title, 0, false, 0, 0.0, "", WeightedAuditMetricStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case WeightedAuditMetricStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(WeightedAuditMetricStatus next)
    {
        switch (Status)
        {
            case WeightedAuditMetricStatus.Proposed:
                switch (next)
                {
                    case WeightedAuditMetricStatus.Withdrawn:
                        return true;
                }
                return false;
            case WeightedAuditMetricStatus.Approved:
                switch (next)
                {
                    case WeightedAuditMetricStatus.Withdrawn:
                        return true;
                }
                return false;
            case WeightedAuditMetricStatus.Rejected:
                return next == WeightedAuditMetricStatus.Withdrawn;
        }
        return false;
    }

    public void MoveTo(WeightedAuditMetricStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"weighted audit metric {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        ExpiresAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code}: {Title}";
    }

    public double EffectiveLoad()
    {
        return Math.Round(Budget * (double)WindowDays, 2);
    }

    public WeightedAuditMetric Copy()
    {
        return new WeightedAuditMetric(Code, Title, ExpiresAt, Approved, WindowDays, Budget, Segment, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Segment);
    }
}
