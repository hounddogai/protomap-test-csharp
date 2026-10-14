namespace Acme.Workforce.Workplace.Occupancy.Checkpoint;

using Acme.Workforce.Kit.Text;

/// <summary>Where a occupancy metric is in its life cycle.</summary>
public enum OccupancyMetricStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>The record of a occupancy metric and its current status.</summary>
public sealed class OccupancyMetric(string code, string name, double rating, string notes, string reviewer, double discount, string externalRef, double minimum, OccupancyMetricStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public double Rating { get; set; } = rating;
    public string Notes { get; set; } = notes;
    public string Reviewer { get; set; } = reviewer;
    public double Discount { get; set; } = discount;
    public string ExternalRef { get; set; } = externalRef;
    public double Minimum { get; set; } = minimum;
    public OccupancyMetricStatus Status { get; set; } = status;

    /// <summary>Starts a occupancy metric in status open.</summary>
    public static OccupancyMetric NewOccupancyMetric(string code, string name)
    {
        return new OccupancyMetric(code, name, 0.0, "", "", 0.0, "", 0.0, OccupancyMetricStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case OccupancyMetricStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(OccupancyMetricStatus next)
    {
        switch (Status)
        {
            case OccupancyMetricStatus.Open:
                switch (next)
                {
                    case OccupancyMetricStatus.Archived:
                        return true;
                }
                return false;
            case OccupancyMetricStatus.InReview:
                switch (next)
                {
                    case OccupancyMetricStatus.Reopened:
                        return true;
                }
                return false;
            case OccupancyMetricStatus.Resolved:
                switch (next)
                {
                    case OccupancyMetricStatus.Archived:
                        return true;
                }
                return false;
            case OccupancyMetricStatus.Reopened:
                return next == OccupancyMetricStatus.Archived;
        }
        return false;
    }

    public void MoveTo(OccupancyMetricStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"occupancy metric {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"occupancy metric"} {Code}";
    }

    public double EffectiveVolume()
    {
        return Math.Round(Rating + Discount, 2);
    }

    public OccupancyMetric Copy()
    {
        return new OccupancyMetric(Code, Name, Rating, Notes, Reviewer, Discount, ExternalRef, Minimum, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
