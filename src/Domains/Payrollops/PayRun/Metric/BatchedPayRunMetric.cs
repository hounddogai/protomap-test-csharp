namespace Acme.Workforce.Payrollops.PayRun.Metric;

using Acme.Workforce.Kit.Text;

/// <summary>Where a batched pay run metric is in its life cycle.</summary>
public enum BatchedPayRunMetricStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>The record of a batched pay run metric and its current status.</summary>
public sealed class BatchedPayRunMetric(string code, string title, string locale, int retries, string grade, List<string> tags, BatchedPayRunMetricStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Locale { get; set; } = locale;
    public int Retries { get; set; } = retries;
    public string Grade { get; set; } = grade;
    public List<string> Tags { get; set; } = tags;
    public BatchedPayRunMetricStatus Status { get; set; } = status;

    /// <summary>Starts a batched pay run metric in status requested.</summary>
    public static BatchedPayRunMetric NewBatchedPayRunMetric(string code, string title)
    {
        return new BatchedPayRunMetric(
            code,
            title,
            "",
            0,
            "",
            new List<string>(),
            BatchedPayRunMetricStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case BatchedPayRunMetricStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(BatchedPayRunMetricStatus next)
    {
        switch (Status)
        {
            case BatchedPayRunMetricStatus.Requested:
                switch (next)
                {
                    case BatchedPayRunMetricStatus.Granted:
                    case BatchedPayRunMetricStatus.Revoked:
                        return true;
                }
                return false;
            case BatchedPayRunMetricStatus.Granted:
                return next == BatchedPayRunMetricStatus.Revoked || next == BatchedPayRunMetricStatus.Expired;
            case BatchedPayRunMetricStatus.Revoked:
                switch (next)
                {
                    case BatchedPayRunMetricStatus.Expired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(BatchedPayRunMetricStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"batched pay run metric {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code}: {Title}";
    }

    public bool HasTag(string tag)
    {
        foreach (var candidate in Tags)
        {
            if (Text.Normalize(candidate) == Text.Normalize(tag))
            {
                return true;
            }
        }
        return false;
    }

    public double MeasuredValue()
    {
        return (double)Retries;
    }

    public BatchedPayRunMetric Copy()
    {
        return new BatchedPayRunMetric(Code, Title, Locale, Retries, Grade, Tags, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Locale);
    }
}
