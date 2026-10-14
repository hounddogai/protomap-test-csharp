namespace Acme.Workforce.Assets.Warranty.Metric;

using Acme.Workforce.Kit.Text;

/// <summary>Where a warranty metric is in its life cycle.</summary>
public enum WarrantyMetricStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>A warranty metric. Its code is unique within a tenant.</summary>
public sealed class WarrantyMetric(string code, string title, string reviewer, string region, int leaveDays, int createdAt, string badge, string category, string checksum, string description, WarrantyMetricStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Reviewer { get; set; } = reviewer;
    public string Region { get; set; } = region;
    public int LeaveDays { get; set; } = leaveDays;
    public int CreatedAt { get; set; } = createdAt;
    public string Badge { get; set; } = badge;
    public string Category { get; set; } = category;
    public string Checksum { get; set; } = checksum;
    public string Description { get; set; } = description;
    public WarrantyMetricStatus Status { get; set; } = status;

    /// <summary>Starts a warranty metric in status new.</summary>
    public static WarrantyMetric NewWarrantyMetric(string code, string title)
    {
        return new WarrantyMetric(code, title, "", "", 0, 0, "", "", "", "", WarrantyMetricStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case WarrantyMetricStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(WarrantyMetricStatus next)
    {
        switch (Status)
        {
            case WarrantyMetricStatus.New:
                switch (next)
                {
                    case WarrantyMetricStatus.Queued:
                        return true;
                }
                return false;
            case WarrantyMetricStatus.Queued:
                switch (next)
                {
                    case WarrantyMetricStatus.Done:
                    case WarrantyMetricStatus.Failed:
                        return true;
                }
                return false;
            case WarrantyMetricStatus.Running:
                switch (next)
                {
                    case WarrantyMetricStatus.Done:
                        return true;
                }
                return false;
            case WarrantyMetricStatus.Done:
                return next == WarrantyMetricStatus.Failed;
        }
        return false;
    }

    public void MoveTo(WarrantyMetricStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"warranty metric {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        CreatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{"warranty metric"} {Code}";
    }

    public double AdjustedTotal()
    {
        return Math.Round((double)CreatedAt * (double)LeaveDays, 2);
    }

    public WarrantyMetric Copy()
    {
        return new WarrantyMetric(
            Code,
            Title,
            Reviewer,
            Region,
            LeaveDays,
            CreatedAt,
            Badge,
            Category,
            Checksum,
            Description,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Region);
    }
}
