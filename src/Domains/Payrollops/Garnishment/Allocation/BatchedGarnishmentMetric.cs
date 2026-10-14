namespace Acme.Workforce.Payrollops.Garnishment.Allocation;

using Acme.Workforce.Kit.Text;

/// <summary>Where a batched garnishment metric is in its life cycle.</summary>
public enum BatchedGarnishmentMetricStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>A batched garnishment metric. Its code is unique within a tenant.</summary>
public sealed class BatchedGarnishmentMetric(string code, string name, string contactEmail, int createdAt, int retries, double hours, int tenureMonths, BatchedGarnishmentMetricStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string ContactEmail { get; set; } = contactEmail;
    public int CreatedAt { get; set; } = createdAt;
    public int Retries { get; set; } = retries;
    public double Hours { get; set; } = hours;
    public int TenureMonths { get; set; } = tenureMonths;
    public BatchedGarnishmentMetricStatus Status { get; set; } = status;

    /// <summary>Starts a batched garnishment metric in status new.</summary>
    public static BatchedGarnishmentMetric NewBatchedGarnishmentMetric(string code, string name)
    {
        return new BatchedGarnishmentMetric(code, name, "", 0, 0, 0.0, 0, BatchedGarnishmentMetricStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case BatchedGarnishmentMetricStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(BatchedGarnishmentMetricStatus next)
    {
        switch (Status)
        {
            case BatchedGarnishmentMetricStatus.New:
                switch (next)
                {
                    case BatchedGarnishmentMetricStatus.Queued:
                    case BatchedGarnishmentMetricStatus.Running:
                        return true;
                }
                return false;
            case BatchedGarnishmentMetricStatus.Queued:
                return next == BatchedGarnishmentMetricStatus.Failed;
            case BatchedGarnishmentMetricStatus.Running:
                return next == BatchedGarnishmentMetricStatus.Failed;
            case BatchedGarnishmentMetricStatus.Done:
                switch (next)
                {
                    case BatchedGarnishmentMetricStatus.Failed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(BatchedGarnishmentMetricStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"batched garnishment metric {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        CreatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} ({Name})";
    }

    public double ProjectedExposure()
    {
        return Math.Round(Hours * (double)CreatedAt, 2);
    }

    public BatchedGarnishmentMetric Copy()
    {
        return new BatchedGarnishmentMetric(
            Code,
            Name,
            ContactEmail,
            CreatedAt,
            Retries,
            Hours,
            TenureMonths,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
