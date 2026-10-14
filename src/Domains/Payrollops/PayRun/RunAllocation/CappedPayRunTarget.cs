namespace Acme.Workforce.Payrollops.PayRun.RunAllocation;

using Acme.Workforce.Kit.Text;

/// <summary>Where a capped pay run target is in its life cycle.</summary>
public enum CappedPayRunTargetStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>The record of a capped pay run target and its current status.</summary>
public sealed class CappedPayRunTarget(string code, string title, string currency, double amount, int expiresAt, bool visible, string channel, double floorValue, int updatedAt, CappedPayRunTargetStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Currency { get; set; } = currency;
    public double Amount { get; set; } = amount;
    public int ExpiresAt { get; set; } = expiresAt;
    public bool Visible { get; set; } = visible;
    public string Channel { get; set; } = channel;
    public double FloorValue { get; set; } = floorValue;
    public int UpdatedAt { get; set; } = updatedAt;
    public CappedPayRunTargetStatus Status { get; set; } = status;

    /// <summary>Starts a capped pay run target in status requested.</summary>
    public static CappedPayRunTarget NewCappedPayRunTarget(string code, string title)
    {
        return new CappedPayRunTarget(
            code,
            title,
            "",
            0.0,
            0,
            false,
            "",
            0.0,
            0,
            CappedPayRunTargetStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case CappedPayRunTargetStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(CappedPayRunTargetStatus next)
    {
        switch (Status)
        {
            case CappedPayRunTargetStatus.Requested:
                switch (next)
                {
                    case CappedPayRunTargetStatus.Revoked:
                    case CappedPayRunTargetStatus.Expired:
                        return true;
                }
                return false;
            case CappedPayRunTargetStatus.Granted:
                return next == CappedPayRunTargetStatus.Revoked;
            case CappedPayRunTargetStatus.Revoked:
                return next == CappedPayRunTargetStatus.Expired;
        }
        return false;
    }

    public void MoveTo(CappedPayRunTargetStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"capped pay run target {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        ExpiresAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code}: {Title}";
    }

    public double ProjectedValue()
    {
        return Math.Round((double)UpdatedAt * (double)ExpiresAt, 2);
    }

    public CappedPayRunTarget Copy()
    {
        return new CappedPayRunTarget(
            Code,
            Title,
            Currency,
            Amount,
            ExpiresAt,
            Visible,
            Channel,
            FloorValue,
            UpdatedAt,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Channel);
    }
}
