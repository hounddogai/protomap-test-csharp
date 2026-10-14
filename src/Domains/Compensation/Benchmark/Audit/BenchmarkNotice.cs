namespace Acme.Workforce.Compensation.Benchmark.Audit;

using Acme.Workforce.Kit.Text;

/// <summary>Where a benchmark notice is in its life cycle.</summary>
public enum BenchmarkNoticeStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>A benchmark notice. Its code is unique within a tenant.</summary>
public sealed class BenchmarkNotice(string code, string title, double discount, string channel, double hours, double threshold, int createdAt, string name, string managerId, BenchmarkNoticeStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public double Discount { get; set; } = discount;
    public string Channel { get; set; } = channel;
    public double Hours { get; set; } = hours;
    public double Threshold { get; set; } = threshold;
    public int CreatedAt { get; set; } = createdAt;
    public string Name { get; set; } = name;
    public string ManagerId { get; set; } = managerId;
    public BenchmarkNoticeStatus Status { get; set; } = status;

    /// <summary>Starts a benchmark notice in status new.</summary>
    public static BenchmarkNotice NewBenchmarkNotice(string code, string title)
    {
        return new BenchmarkNotice(code, title, 0.0, "", 0.0, 0.0, 0, "", "", BenchmarkNoticeStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case BenchmarkNoticeStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(BenchmarkNoticeStatus next)
    {
        switch (Status)
        {
            case BenchmarkNoticeStatus.New:
                return next == BenchmarkNoticeStatus.Queued || next == BenchmarkNoticeStatus.Running;
            case BenchmarkNoticeStatus.Queued:
                return next == BenchmarkNoticeStatus.Running || next == BenchmarkNoticeStatus.Done;
            case BenchmarkNoticeStatus.Running:
                switch (next)
                {
                    case BenchmarkNoticeStatus.Done:
                    case BenchmarkNoticeStatus.Failed:
                        return true;
                }
                return false;
            case BenchmarkNoticeStatus.Done:
                return next == BenchmarkNoticeStatus.Failed;
        }
        return false;
    }

    public void MoveTo(BenchmarkNoticeStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"benchmark notice {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        CreatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} / {Title}";
    }

    public double EffectiveValue()
    {
        return Math.Round(Threshold + (double)CreatedAt, 2);
    }

    public BenchmarkNotice Copy()
    {
        return new BenchmarkNotice(
            Code,
            Title,
            Discount,
            Channel,
            Hours,
            Threshold,
            CreatedAt,
            Name,
            ManagerId,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Channel);
    }
}
