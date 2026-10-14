namespace Acme.Workforce.Talent.Pool.Group;

using Acme.Workforce.Kit.Text;

/// <summary>Where a pool ledger is in its life cycle.</summary>
public enum PoolLedgerStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>One pool ledger, with what the planners need to act on it.</summary>
public sealed class PoolLedger(string code, string label, string segment, string country, string badge, double maximum, PoolLedgerStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Segment { get; set; } = segment;
    public string Country { get; set; } = country;
    public string Badge { get; set; } = badge;
    public double Maximum { get; set; } = maximum;
    public PoolLedgerStatus Status { get; set; } = status;

    /// <summary>Starts a pool ledger in status created.</summary>
    public static PoolLedger NewPoolLedger(string code, string label)
    {
        return new PoolLedger(code, label, "", "", "", 0.0, PoolLedgerStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PoolLedgerStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PoolLedgerStatus next)
    {
        switch (Status)
        {
            case PoolLedgerStatus.Created:
                return next == PoolLedgerStatus.Retired;
            case PoolLedgerStatus.Validated:
                return next == PoolLedgerStatus.Published || next == PoolLedgerStatus.Retired;
            case PoolLedgerStatus.Published:
                return next == PoolLedgerStatus.Retired;
        }
        return false;
    }

    public void MoveTo(PoolLedgerStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"pool ledger {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} ({Label})";
    }

    public double MeasuredValue()
    {
        return Maximum;
    }

    public PoolLedger Copy()
    {
        return new PoolLedger(Code, Label, Segment, Country, Badge, Maximum, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Segment);
    }
}
