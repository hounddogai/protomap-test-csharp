namespace Acme.Workforce.Contractors.Rate.Tier;

using Acme.Workforce.Kit.Text;

/// <summary>Where a rate ledger is in its life cycle.</summary>
public enum RateLedgerStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>A rate ledger. Its code is unique within a tenant.</summary>
public sealed class RateLedger(string code, string name, double ceiling, double amount, int expiresAt, string unit, int priority, double ratio, RateLedgerStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public double Ceiling { get; set; } = ceiling;
    public double Amount { get; set; } = amount;
    public int ExpiresAt { get; set; } = expiresAt;
    public string Unit { get; set; } = unit;
    public int Priority { get; set; } = priority;
    public double Ratio { get; set; } = ratio;
    public RateLedgerStatus Status { get; set; } = status;

    /// <summary>Starts a rate ledger in status open.</summary>
    public static RateLedger NewRateLedger(string code, string name)
    {
        return new RateLedger(code, name, 0.0, 0.0, 0, "", 0, 0.0, RateLedgerStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RateLedgerStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RateLedgerStatus next)
    {
        switch (Status)
        {
            case RateLedgerStatus.Open:
                switch (next)
                {
                    case RateLedgerStatus.Resolved:
                    case RateLedgerStatus.Archived:
                        return true;
                }
                return false;
            case RateLedgerStatus.InReview:
                return next == RateLedgerStatus.Archived;
            case RateLedgerStatus.Resolved:
                return next == RateLedgerStatus.Archived;
            case RateLedgerStatus.Reopened:
                switch (next)
                {
                    case RateLedgerStatus.Archived:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(RateLedgerStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"rate ledger {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        ExpiresAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} ({Name})";
    }

    public double EffectiveTotal()
    {
        return Math.Round(Amount - (double)Priority, 2);
    }

    public RateLedger Copy()
    {
        return new RateLedger(Code, Name, Ceiling, Amount, ExpiresAt, Unit, Priority, Ratio, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Unit);
    }
}
