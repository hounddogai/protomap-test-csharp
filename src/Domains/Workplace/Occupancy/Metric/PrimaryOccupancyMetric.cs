namespace Acme.Workforce.Workplace.Occupancy.Metric;

using Acme.Workforce.Kit.Text;

/// <summary>Where a primary occupancy metric is in its life cycle.</summary>
public enum PrimaryOccupancyMetricStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>A primary occupancy metric as the occupancy team keeps it.</summary>
public sealed class PrimaryOccupancyMetric(string code, string label, string externalRef, string badge, string ownerId, bool visible, string contactEmail, string reviewer, string phone, double amount, PrimaryOccupancyMetricStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string ExternalRef { get; set; } = externalRef;
    public string Badge { get; set; } = badge;
    public string OwnerId { get; set; } = ownerId;
    public bool Visible { get; set; } = visible;
    public string ContactEmail { get; set; } = contactEmail;
    public string Reviewer { get; set; } = reviewer;
    public string Phone { get; set; } = phone;
    public double Amount { get; set; } = amount;
    public PrimaryOccupancyMetricStatus Status { get; set; } = status;

    /// <summary>Starts a primary occupancy metric in status open.</summary>
    public static PrimaryOccupancyMetric NewPrimaryOccupancyMetric(string code, string label)
    {
        return new PrimaryOccupancyMetric(
            code,
            label,
            "",
            "",
            "",
            false,
            "",
            "",
            "",
            0.0,
            PrimaryOccupancyMetricStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PrimaryOccupancyMetricStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PrimaryOccupancyMetricStatus next)
    {
        switch (Status)
        {
            case PrimaryOccupancyMetricStatus.Open:
                switch (next)
                {
                    case PrimaryOccupancyMetricStatus.Reopened:
                        return true;
                }
                return false;
            case PrimaryOccupancyMetricStatus.InReview:
                return next == PrimaryOccupancyMetricStatus.Resolved || next == PrimaryOccupancyMetricStatus.Archived;
            case PrimaryOccupancyMetricStatus.Resolved:
                return next == PrimaryOccupancyMetricStatus.Reopened || next == PrimaryOccupancyMetricStatus.Archived;
            case PrimaryOccupancyMetricStatus.Reopened:
                switch (next)
                {
                    case PrimaryOccupancyMetricStatus.Archived:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(PrimaryOccupancyMetricStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"primary occupancy metric {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} ({Label})";
    }

    public double MeasuredValue()
    {
        return Amount;
    }

    public PrimaryOccupancyMetric Copy()
    {
        return new PrimaryOccupancyMetric(
            Code,
            Label,
            ExternalRef,
            Badge,
            OwnerId,
            Visible,
            ContactEmail,
            Reviewer,
            Phone,
            Amount,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
