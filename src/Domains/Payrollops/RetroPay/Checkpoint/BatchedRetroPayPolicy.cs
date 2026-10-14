namespace Acme.Workforce.Payrollops.RetroPay.Checkpoint;

using Acme.Workforce.Kit.Text;

/// <summary>Where a batched retro pay policy is in its life cycle.</summary>
public enum BatchedRetroPayPolicyStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>A batched retro pay policy as the retro pay team keeps it.</summary>
public sealed class BatchedRetroPayPolicy(string code, string title, double rating, int windowDays, string locale, string currency, int createdAt, string source, int expiresAt, double amount, BatchedRetroPayPolicyStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public double Rating { get; set; } = rating;
    public int WindowDays { get; set; } = windowDays;
    public string Locale { get; set; } = locale;
    public string Currency { get; set; } = currency;
    public int CreatedAt { get; set; } = createdAt;
    public string Source { get; set; } = source;
    public int ExpiresAt { get; set; } = expiresAt;
    public double Amount { get; set; } = amount;
    public BatchedRetroPayPolicyStatus Status { get; set; } = status;

    /// <summary>Starts a batched retro pay policy in status idle.</summary>
    public static BatchedRetroPayPolicy NewBatchedRetroPayPolicy(string code, string title)
    {
        return new BatchedRetroPayPolicy(
            code,
            title,
            0.0,
            0,
            "",
            "",
            0,
            "",
            0,
            0.0,
            BatchedRetroPayPolicyStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case BatchedRetroPayPolicyStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(BatchedRetroPayPolicyStatus next)
    {
        switch (Status)
        {
            case BatchedRetroPayPolicyStatus.Idle:
                return next == BatchedRetroPayPolicyStatus.Ready || next == BatchedRetroPayPolicyStatus.Draining;
            case BatchedRetroPayPolicyStatus.Warming:
                switch (next)
                {
                    case BatchedRetroPayPolicyStatus.Draining:
                    case BatchedRetroPayPolicyStatus.Stopped:
                        return true;
                }
                return false;
            case BatchedRetroPayPolicyStatus.Ready:
                return next == BatchedRetroPayPolicyStatus.Draining || next == BatchedRetroPayPolicyStatus.Stopped;
            case BatchedRetroPayPolicyStatus.Draining:
                switch (next)
                {
                    case BatchedRetroPayPolicyStatus.Stopped:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(BatchedRetroPayPolicyStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"batched retro pay policy {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        CreatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} [{Title}]";
    }

    public double WeightedVolume()
    {
        return Math.Round((double)WindowDays * Rating, 2);
    }

    public BatchedRetroPayPolicy Copy()
    {
        return new BatchedRetroPayPolicy(
            Code,
            Title,
            Rating,
            WindowDays,
            Locale,
            Currency,
            CreatedAt,
            Source,
            ExpiresAt,
            Amount,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Locale);
    }
}
