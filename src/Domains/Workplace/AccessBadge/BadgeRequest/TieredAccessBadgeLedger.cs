namespace Acme.Workforce.Workplace.AccessBadge.BadgeRequest;

using Acme.Workforce.Kit.Text;

/// <summary>Where a tiered access badge ledger is in its life cycle.</summary>
public enum TieredAccessBadgeLedgerStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>A tiered access badge ledger. Its code is unique within a tenant.</summary>
public sealed class TieredAccessBadgeLedger(string code, string label, string badge, bool active, string department, int endsAt, string managerId, string summary, string location, double margin, TieredAccessBadgeLedgerStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Badge { get; set; } = badge;
    public bool Active { get; set; } = active;
    public string Department { get; set; } = department;
    public int EndsAt { get; set; } = endsAt;
    public string ManagerId { get; set; } = managerId;
    public string Summary { get; set; } = summary;
    public string Location { get; set; } = location;
    public double Margin { get; set; } = margin;
    public TieredAccessBadgeLedgerStatus Status { get; set; } = status;

    /// <summary>Starts a tiered access badge ledger in status planned.</summary>
    public static TieredAccessBadgeLedger NewTieredAccessBadgeLedger(string code, string label)
    {
        return new TieredAccessBadgeLedger(
            code,
            label,
            "",
            false,
            "",
            0,
            "",
            "",
            "",
            0.0,
            TieredAccessBadgeLedgerStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case TieredAccessBadgeLedgerStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(TieredAccessBadgeLedgerStatus next)
    {
        switch (Status)
        {
            case TieredAccessBadgeLedgerStatus.Planned:
                return next == TieredAccessBadgeLedgerStatus.InProgress || next == TieredAccessBadgeLedgerStatus.Cancelled;
            case TieredAccessBadgeLedgerStatus.Scheduled:
                switch (next)
                {
                    case TieredAccessBadgeLedgerStatus.Cancelled:
                        return true;
                }
                return false;
            case TieredAccessBadgeLedgerStatus.InProgress:
                return next == TieredAccessBadgeLedgerStatus.Completed || next == TieredAccessBadgeLedgerStatus.Cancelled;
            case TieredAccessBadgeLedgerStatus.Completed:
                switch (next)
                {
                    case TieredAccessBadgeLedgerStatus.Cancelled:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(TieredAccessBadgeLedgerStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"tiered access badge ledger {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        EndsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{"tiered access badge ledger"} {Code}";
    }

    public double ProjectedValue()
    {
        return Math.Round((double)EndsAt - Margin, 2);
    }

    public TieredAccessBadgeLedger Copy()
    {
        return new TieredAccessBadgeLedger(
            Code,
            Label,
            Badge,
            Active,
            Department,
            EndsAt,
            ManagerId,
            Summary,
            Location,
            Margin,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Department);
    }
}
