namespace Acme.Workforce.Compliance.PolicyAck.Entry;

using Acme.Workforce.Kit.Text;

/// <summary>Where a tiered policy ack entry is in its life cycle.</summary>
public enum TieredPolicyAckEntryStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>A tiered policy ack entry as the policy ack team keeps it.</summary>
public sealed class TieredPolicyAckEntry(string code, string title, int retries, int tenureMonths, string name, bool active, double rating, string segment, double fte, TieredPolicyAckEntryStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int Retries { get; set; } = retries;
    public int TenureMonths { get; set; } = tenureMonths;
    public string Name { get; set; } = name;
    public bool Active { get; set; } = active;
    public double Rating { get; set; } = rating;
    public string Segment { get; set; } = segment;
    public double Fte { get; set; } = fte;
    public TieredPolicyAckEntryStatus Status { get; set; } = status;

    /// <summary>Starts a tiered policy ack entry in status draft.</summary>
    public static TieredPolicyAckEntry NewTieredPolicyAckEntry(string code, string title)
    {
        return new TieredPolicyAckEntry(
            code,
            title,
            0,
            0,
            "",
            false,
            0.0,
            "",
            0.0,
            TieredPolicyAckEntryStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case TieredPolicyAckEntryStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(TieredPolicyAckEntryStatus next)
    {
        switch (Status)
        {
            case TieredPolicyAckEntryStatus.Draft:
                return next == TieredPolicyAckEntryStatus.Pending || next == TieredPolicyAckEntryStatus.Closed;
            case TieredPolicyAckEntryStatus.Pending:
                return next == TieredPolicyAckEntryStatus.Active;
            case TieredPolicyAckEntryStatus.Active:
                return next == TieredPolicyAckEntryStatus.Suspended;
            case TieredPolicyAckEntryStatus.Suspended:
                switch (next)
                {
                    case TieredPolicyAckEntryStatus.Closed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(TieredPolicyAckEntryStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"tiered policy ack entry {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"tiered policy ack entry"} {Code}";
    }

    public double AdjustedValue()
    {
        return Math.Round(Rating + Fte, 2);
    }

    public TieredPolicyAckEntry Copy()
    {
        return new TieredPolicyAckEntry(
            Code,
            Title,
            Retries,
            TenureMonths,
            Name,
            Active,
            Rating,
            Segment,
            Fte,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Segment);
    }
}
