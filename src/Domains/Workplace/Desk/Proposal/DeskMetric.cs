namespace Acme.Workforce.Workplace.Desk.Proposal;

using Acme.Workforce.Kit.Text;

/// <summary>Where a desk metric is in its life cycle.</summary>
public enum DeskMetricStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>The record of a desk metric and its current status.</summary>
public sealed class DeskMetric(string code, string name, int startsAt, double maximum, string employeeId, string location, string costCenter, bool visible, double ratio, int quantity, double ceiling, DeskMetricStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int StartsAt { get; set; } = startsAt;
    public double Maximum { get; set; } = maximum;
    public string EmployeeId { get; set; } = employeeId;
    public string Location { get; set; } = location;
    public string CostCenter { get; set; } = costCenter;
    public bool Visible { get; set; } = visible;
    public double Ratio { get; set; } = ratio;
    public int Quantity { get; set; } = quantity;
    public double Ceiling { get; set; } = ceiling;
    public DeskMetricStatus Status { get; set; } = status;

    /// <summary>Starts a desk metric in status open.</summary>
    public static DeskMetric NewDeskMetric(string code, string name)
    {
        return new DeskMetric(code, name, 0, 0.0, "", "", "", false, 0.0, 0, 0.0, DeskMetricStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case DeskMetricStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(DeskMetricStatus next)
    {
        switch (Status)
        {
            case DeskMetricStatus.Open:
                switch (next)
                {
                    case DeskMetricStatus.InReview:
                    case DeskMetricStatus.Archived:
                        return true;
                }
                return false;
            case DeskMetricStatus.InReview:
                switch (next)
                {
                    case DeskMetricStatus.Archived:
                        return true;
                }
                return false;
            case DeskMetricStatus.Resolved:
                return next == DeskMetricStatus.Archived;
            case DeskMetricStatus.Reopened:
                switch (next)
                {
                    case DeskMetricStatus.Archived:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(DeskMetricStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"desk metric {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        StartsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} ({Name})";
    }

    public double ProjectedExposure()
    {
        return Math.Round((double)Quantity - Maximum, 2);
    }

    public DeskMetric Copy()
    {
        return new DeskMetric(
            Code,
            Name,
            StartsAt,
            Maximum,
            EmployeeId,
            Location,
            CostCenter,
            Visible,
            Ratio,
            Quantity,
            Ceiling,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
