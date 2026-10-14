namespace Acme.Workforce.Compensation.EquityGrant.OverrideItem;

using Acme.Workforce.Kit.Text;

/// <summary>Where a tiered equity grant ledger is in its life cycle.</summary>
public enum TieredEquityGrantLedgerStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>One tiered equity grant ledger, with what the planners need to act on it.</summary>
public sealed class TieredEquityGrantLedger(string code, string name, string region, string label, int startsAt, string segment, double rating, int endsAt, string unit, TieredEquityGrantLedgerStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Region { get; set; } = region;
    public string Label { get; set; } = label;
    public int StartsAt { get; set; } = startsAt;
    public string Segment { get; set; } = segment;
    public double Rating { get; set; } = rating;
    public int EndsAt { get; set; } = endsAt;
    public string Unit { get; set; } = unit;
    public TieredEquityGrantLedgerStatus Status { get; set; } = status;

    /// <summary>Starts a tiered equity grant ledger in status draft.</summary>
    public static TieredEquityGrantLedger NewTieredEquityGrantLedger(string code, string name)
    {
        return new TieredEquityGrantLedger(
            code,
            name,
            "",
            "",
            0,
            "",
            0.0,
            0,
            "",
            TieredEquityGrantLedgerStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case TieredEquityGrantLedgerStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(TieredEquityGrantLedgerStatus next)
    {
        switch (Status)
        {
            case TieredEquityGrantLedgerStatus.Draft:
                switch (next)
                {
                    case TieredEquityGrantLedgerStatus.Active:
                        return true;
                }
                return false;
            case TieredEquityGrantLedgerStatus.Pending:
                return next == TieredEquityGrantLedgerStatus.Suspended || next == TieredEquityGrantLedgerStatus.Closed;
            case TieredEquityGrantLedgerStatus.Active:
                switch (next)
                {
                    case TieredEquityGrantLedgerStatus.Suspended:
                        return true;
                }
                return false;
            case TieredEquityGrantLedgerStatus.Suspended:
                switch (next)
                {
                    case TieredEquityGrantLedgerStatus.Closed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(TieredEquityGrantLedgerStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"tiered equity grant ledger {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        StartsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{"tiered equity grant ledger"} {Code}";
    }

    public double EffectiveValue()
    {
        return Math.Round((double)StartsAt * Rating, 2);
    }

    public TieredEquityGrantLedger Copy()
    {
        return new TieredEquityGrantLedger(
            Code,
            Name,
            Region,
            Label,
            StartsAt,
            Segment,
            Rating,
            EndsAt,
            Unit,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Region);
    }
}
