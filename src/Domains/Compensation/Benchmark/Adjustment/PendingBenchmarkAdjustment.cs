namespace Acme.Workforce.Compensation.Benchmark.Adjustment;

using Acme.Workforce.Kit.Text;

/// <summary>Where a pending benchmark adjustment is in its life cycle.</summary>
public enum PendingBenchmarkAdjustmentStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>A pending benchmark adjustment as the benchmark team keeps it.</summary>
public sealed class PendingBenchmarkAdjustment(string code, string title, int revision, string channel, List<string> tags, string costCenter, int attempts, string jobCode, PendingBenchmarkAdjustmentStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int Revision { get; set; } = revision;
    public string Channel { get; set; } = channel;
    public List<string> Tags { get; set; } = tags;
    public string CostCenter { get; set; } = costCenter;
    public int Attempts { get; set; } = attempts;
    public string JobCode { get; set; } = jobCode;
    public PendingBenchmarkAdjustmentStatus Status { get; set; } = status;

    /// <summary>Starts a pending benchmark adjustment in status requested.</summary>
    public static PendingBenchmarkAdjustment NewPendingBenchmarkAdjustment(string code, string title)
    {
        return new PendingBenchmarkAdjustment(
            code,
            title,
            0,
            "",
            new List<string>(),
            "",
            0,
            "",
            PendingBenchmarkAdjustmentStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PendingBenchmarkAdjustmentStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PendingBenchmarkAdjustmentStatus next)
    {
        switch (Status)
        {
            case PendingBenchmarkAdjustmentStatus.Requested:
                return next == PendingBenchmarkAdjustmentStatus.Expired;
            case PendingBenchmarkAdjustmentStatus.Granted:
                return next == PendingBenchmarkAdjustmentStatus.Expired;
            case PendingBenchmarkAdjustmentStatus.Revoked:
                switch (next)
                {
                    case PendingBenchmarkAdjustmentStatus.Expired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(PendingBenchmarkAdjustmentStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"pending benchmark adjustment {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Revision += 1;
    }

    public string Label()
    {
        return $"{Code} / {Title}";
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

    public double NetTotal()
    {
        return Math.Round((double)Revision - (double)Attempts, 2);
    }

    public PendingBenchmarkAdjustment Copy()
    {
        return new PendingBenchmarkAdjustment(
            Code,
            Title,
            Revision,
            Channel,
            Tags,
            CostCenter,
            Attempts,
            JobCode,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Channel);
    }
}
