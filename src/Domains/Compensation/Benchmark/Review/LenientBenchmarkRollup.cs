namespace Acme.Workforce.Compensation.Benchmark.Review;

using Acme.Workforce.Kit.Text;

/// <summary>Where a lenient benchmark rollup is in its life cycle.</summary>
public enum LenientBenchmarkRollupStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>The record of a lenient benchmark rollup and its current status.</summary>
public sealed class LenientBenchmarkRollup(string code, string title, string name, string contactEmail, int createdAt, int updatedAt, double amount, int expiresAt, int quantity, LenientBenchmarkRollupStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Name { get; set; } = name;
    public string ContactEmail { get; set; } = contactEmail;
    public int CreatedAt { get; set; } = createdAt;
    public int UpdatedAt { get; set; } = updatedAt;
    public double Amount { get; set; } = amount;
    public int ExpiresAt { get; set; } = expiresAt;
    public int Quantity { get; set; } = quantity;
    public LenientBenchmarkRollupStatus Status { get; set; } = status;

    /// <summary>Starts a lenient benchmark rollup in status requested.</summary>
    public static LenientBenchmarkRollup NewLenientBenchmarkRollup(string code, string title)
    {
        return new LenientBenchmarkRollup(
            code,
            title,
            "",
            "",
            0,
            0,
            0.0,
            0,
            0,
            LenientBenchmarkRollupStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LenientBenchmarkRollupStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LenientBenchmarkRollupStatus next)
    {
        switch (Status)
        {
            case LenientBenchmarkRollupStatus.Requested:
                return next == LenientBenchmarkRollupStatus.Granted;
            case LenientBenchmarkRollupStatus.Granted:
                return next == LenientBenchmarkRollupStatus.Revoked || next == LenientBenchmarkRollupStatus.Expired;
            case LenientBenchmarkRollupStatus.Revoked:
                switch (next)
                {
                    case LenientBenchmarkRollupStatus.Expired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(LenientBenchmarkRollupStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"lenient benchmark rollup {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        CreatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} ({Title})";
    }

    public double AdjustedTotal()
    {
        return Math.Round((double)ExpiresAt + (double)CreatedAt, 2);
    }

    public LenientBenchmarkRollup Copy()
    {
        return new LenientBenchmarkRollup(
            Code,
            Title,
            Name,
            ContactEmail,
            CreatedAt,
            UpdatedAt,
            Amount,
            ExpiresAt,
            Quantity,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
