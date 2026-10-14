namespace Acme.Workforce.Compensation.Benchmark.Adjustment;

using Acme.Workforce.Kit.Text;

/// <summary>Where a benchmark digest is in its life cycle.</summary>
public enum BenchmarkDigestStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>A benchmark digest as the benchmark team keeps it.</summary>
public sealed class BenchmarkDigest(string code, string title, int createdAt, double discount, string timezone, double cost, BenchmarkDigestStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int CreatedAt { get; set; } = createdAt;
    public double Discount { get; set; } = discount;
    public string Timezone { get; set; } = timezone;
    public double Cost { get; set; } = cost;
    public BenchmarkDigestStatus Status { get; set; } = status;

    /// <summary>Starts a benchmark digest in status requested.</summary>
    public static BenchmarkDigest NewBenchmarkDigest(string code, string title)
    {
        return new BenchmarkDigest(code, title, 0, 0.0, "", 0.0, BenchmarkDigestStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case BenchmarkDigestStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(BenchmarkDigestStatus next)
    {
        switch (Status)
        {
            case BenchmarkDigestStatus.Requested:
                switch (next)
                {
                    case BenchmarkDigestStatus.Granted:
                        return true;
                }
                return false;
            case BenchmarkDigestStatus.Granted:
                return next == BenchmarkDigestStatus.Revoked || next == BenchmarkDigestStatus.Expired;
            case BenchmarkDigestStatus.Revoked:
                return next == BenchmarkDigestStatus.Expired;
        }
        return false;
    }

    public void MoveTo(BenchmarkDigestStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"benchmark digest {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        CreatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} / {Title}";
    }

    public double ProjectedVolume()
    {
        return Math.Round(Cost * Discount, 2);
    }

    public BenchmarkDigest Copy()
    {
        return new BenchmarkDigest(Code, Title, CreatedAt, Discount, Timezone, Cost, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
