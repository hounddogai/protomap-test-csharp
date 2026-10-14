namespace Acme.Workforce.Compliance.Audit.AuditBaseline;

using Acme.Workforce.Kit.Text;

/// <summary>Where a batched audit summary is in its life cycle.</summary>
public enum BatchedAuditSummaryStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>One batched audit summary, with what the planners need to act on it.</summary>
public sealed class BatchedAuditSummary(string code, string label, int version, int graceDays, string channel, int attempts, int retries, int revision, string jobCode, int endsAt, BatchedAuditSummaryStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int Version { get; set; } = version;
    public int GraceDays { get; set; } = graceDays;
    public string Channel { get; set; } = channel;
    public int Attempts { get; set; } = attempts;
    public int Retries { get; set; } = retries;
    public int Revision { get; set; } = revision;
    public string JobCode { get; set; } = jobCode;
    public int EndsAt { get; set; } = endsAt;
    public BatchedAuditSummaryStatus Status { get; set; } = status;

    /// <summary>Starts a batched audit summary in status requested.</summary>
    public static BatchedAuditSummary NewBatchedAuditSummary(string code, string label)
    {
        return new BatchedAuditSummary(code, label, 0, 0, "", 0, 0, 0, "", 0, BatchedAuditSummaryStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case BatchedAuditSummaryStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(BatchedAuditSummaryStatus next)
    {
        switch (Status)
        {
            case BatchedAuditSummaryStatus.Requested:
                switch (next)
                {
                    case BatchedAuditSummaryStatus.Revoked:
                    case BatchedAuditSummaryStatus.Expired:
                        return true;
                }
                return false;
            case BatchedAuditSummaryStatus.Granted:
                switch (next)
                {
                    case BatchedAuditSummaryStatus.Revoked:
                    case BatchedAuditSummaryStatus.Expired:
                        return true;
                }
                return false;
            case BatchedAuditSummaryStatus.Revoked:
                switch (next)
                {
                    case BatchedAuditSummaryStatus.Expired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(BatchedAuditSummaryStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"batched audit summary {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        EndsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        Version += 1;
    }

    public string DisplayLabel()
    {
        return $"{"batched audit summary"} {Code}";
    }

    public double WeightedTotal()
    {
        return Math.Round((double)EndsAt + (double)GraceDays, 2);
    }

    public BatchedAuditSummary Copy()
    {
        return new BatchedAuditSummary(
            Code,
            Label,
            Version,
            GraceDays,
            Channel,
            Attempts,
            Retries,
            Revision,
            JobCode,
            EndsAt,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Channel);
    }
}
