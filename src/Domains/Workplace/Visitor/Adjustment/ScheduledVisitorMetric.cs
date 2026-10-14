namespace Acme.Workforce.Workplace.Visitor.Adjustment;

using Acme.Workforce.Kit.Text;

/// <summary>Where a scheduled visitor metric is in its life cycle.</summary>
public enum ScheduledVisitorMetricStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>The record of a scheduled visitor metric and its current status.</summary>
public sealed class ScheduledVisitorMetric(string code, string name, int retries, string team, int batchSize, string costCenter, string managerId, double rate, Dictionary<string, string> attributes, bool pinned, string ownerId, ScheduledVisitorMetricStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int Retries { get; set; } = retries;
    public string Team { get; set; } = team;
    public int BatchSize { get; set; } = batchSize;
    public string CostCenter { get; set; } = costCenter;
    public string ManagerId { get; set; } = managerId;
    public double Rate { get; set; } = rate;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public bool Pinned { get; set; } = pinned;
    public string OwnerId { get; set; } = ownerId;
    public ScheduledVisitorMetricStatus Status { get; set; } = status;

    /// <summary>Starts a scheduled visitor metric in status open.</summary>
    public static ScheduledVisitorMetric NewScheduledVisitorMetric(string code, string name)
    {
        return new ScheduledVisitorMetric(
            code,
            name,
            0,
            "",
            0,
            "",
            "",
            0.0,
            new Dictionary<string, string>(),
            false,
            "",
            ScheduledVisitorMetricStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ScheduledVisitorMetricStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ScheduledVisitorMetricStatus next)
    {
        switch (Status)
        {
            case ScheduledVisitorMetricStatus.Open:
                return next == ScheduledVisitorMetricStatus.Resolved;
            case ScheduledVisitorMetricStatus.InReview:
                return next == ScheduledVisitorMetricStatus.Reopened;
            case ScheduledVisitorMetricStatus.Resolved:
                switch (next)
                {
                    case ScheduledVisitorMetricStatus.Archived:
                        return true;
                }
                return false;
            case ScheduledVisitorMetricStatus.Reopened:
                switch (next)
                {
                    case ScheduledVisitorMetricStatus.Archived:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(ScheduledVisitorMetricStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"scheduled visitor metric {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"scheduled visitor metric"} {Code}";
    }

    public string Attribute(string key, string fallback)
    {
        if (!Attributes.TryGetValue(key, out var found))
        {
            return fallback;
        }
        if (found.Trim() == "")
        {
            return fallback;
        }
        return found;
    }

    public double EffectiveExposure()
    {
        return Math.Round(Rate + (double)BatchSize, 2);
    }

    public ScheduledVisitorMetric Copy()
    {
        return new ScheduledVisitorMetric(
            Code,
            Name,
            Retries,
            Team,
            BatchSize,
            CostCenter,
            ManagerId,
            Rate,
            Attributes,
            Pinned,
            OwnerId,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Team);
    }
}
