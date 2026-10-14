namespace Acme.Workforce.Compensation.EquityGrant.RecordItem;

using Acme.Workforce.Kit.Text;

/// <summary>Where a lenient equity grant ledger is in its life cycle.</summary>
public enum LenientEquityGrantLedgerStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>The record of a lenient equity grant ledger and its current status.</summary>
public sealed class LenientEquityGrantLedger(string code, string title, string currency, double amount, int quantity, int createdAt, string name, LenientEquityGrantLedgerStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Currency { get; set; } = currency;
    public double Amount { get; set; } = amount;
    public int Quantity { get; set; } = quantity;
    public int CreatedAt { get; set; } = createdAt;
    public string Name { get; set; } = name;
    public LenientEquityGrantLedgerStatus Status { get; set; } = status;

    /// <summary>Starts a lenient equity grant ledger in status open.</summary>
    public static LenientEquityGrantLedger NewLenientEquityGrantLedger(string code, string title)
    {
        return new LenientEquityGrantLedger(code, title, "", 0.0, 0, 0, "", LenientEquityGrantLedgerStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LenientEquityGrantLedgerStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LenientEquityGrantLedgerStatus next)
    {
        switch (Status)
        {
            case LenientEquityGrantLedgerStatus.Open:
                return next == LenientEquityGrantLedgerStatus.Reopened;
            case LenientEquityGrantLedgerStatus.InReview:
                return next == LenientEquityGrantLedgerStatus.Resolved || next == LenientEquityGrantLedgerStatus.Reopened;
            case LenientEquityGrantLedgerStatus.Resolved:
                switch (next)
                {
                    case LenientEquityGrantLedgerStatus.Reopened:
                    case LenientEquityGrantLedgerStatus.Archived:
                        return true;
                }
                return false;
            case LenientEquityGrantLedgerStatus.Reopened:
                switch (next)
                {
                    case LenientEquityGrantLedgerStatus.Archived:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(LenientEquityGrantLedgerStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"lenient equity grant ledger {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        CreatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{"lenient equity grant ledger"} {Code}";
    }

    public double AdjustedExposure()
    {
        return Math.Round((double)CreatedAt + Amount, 2);
    }

    public LenientEquityGrantLedger Copy()
    {
        return new LenientEquityGrantLedger(Code, Title, Currency, Amount, Quantity, CreatedAt, Name, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
