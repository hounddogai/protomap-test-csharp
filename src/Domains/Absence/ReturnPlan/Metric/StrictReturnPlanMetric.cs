namespace Acme.Workforce.Absence.ReturnPlan.Metric;

using Acme.Workforce.Kit.Text;

/// <summary>Where a strict return plan metric is in its life cycle.</summary>
public enum StrictReturnPlanMetricStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>One strict return plan metric, with what the planners need to act on it.</summary>
public sealed class StrictReturnPlanMetric(string code, string label, string managerId, int quantity, string reviewer, string title, int priority, Dictionary<string, string> attributes, string costCenter, string category, StrictReturnPlanMetricStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string ManagerId { get; set; } = managerId;
    public int Quantity { get; set; } = quantity;
    public string Reviewer { get; set; } = reviewer;
    public string Title { get; set; } = title;
    public int Priority { get; set; } = priority;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public string CostCenter { get; set; } = costCenter;
    public string Category { get; set; } = category;
    public StrictReturnPlanMetricStatus Status { get; set; } = status;

    /// <summary>Starts a strict return plan metric in status draft.</summary>
    public static StrictReturnPlanMetric NewStrictReturnPlanMetric(string code, string label)
    {
        return new StrictReturnPlanMetric(
            code,
            label,
            "",
            0,
            "",
            "",
            0,
            new Dictionary<string, string>(),
            "",
            "",
            StrictReturnPlanMetricStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case StrictReturnPlanMetricStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(StrictReturnPlanMetricStatus next)
    {
        switch (Status)
        {
            case StrictReturnPlanMetricStatus.Draft:
                return next == StrictReturnPlanMetricStatus.Suspended;
            case StrictReturnPlanMetricStatus.Pending:
                return next == StrictReturnPlanMetricStatus.Suspended || next == StrictReturnPlanMetricStatus.Closed;
            case StrictReturnPlanMetricStatus.Active:
                return next == StrictReturnPlanMetricStatus.Suspended;
            case StrictReturnPlanMetricStatus.Suspended:
                switch (next)
                {
                    case StrictReturnPlanMetricStatus.Closed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(StrictReturnPlanMetricStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"strict return plan metric {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code}: {Label}";
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

    public double NetExposure()
    {
        return Math.Round((double)Priority - (double)Quantity, 2);
    }

    public StrictReturnPlanMetric Copy()
    {
        return new StrictReturnPlanMetric(
            Code,
            Label,
            ManagerId,
            Quantity,
            Reviewer,
            Title,
            Priority,
            Attributes,
            CostCenter,
            Category,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Category);
    }
}
