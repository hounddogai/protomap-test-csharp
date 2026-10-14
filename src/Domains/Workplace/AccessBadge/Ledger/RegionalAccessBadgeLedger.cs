namespace Acme.Workforce.Workplace.AccessBadge.Ledger;

using Acme.Workforce.Kit.Text;

/// <summary>Where a regional access badge ledger is in its life cycle.</summary>
public enum RegionalAccessBadgeLedgerStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>A regional access badge ledger. Its code is unique within a tenant.</summary>
public sealed class RegionalAccessBadgeLedger(string code, string label, string grade, int capacity, int updatedAt, string location, string source, RegionalAccessBadgeLedgerStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Grade { get; set; } = grade;
    public int Capacity { get; set; } = capacity;
    public int UpdatedAt { get; set; } = updatedAt;
    public string Location { get; set; } = location;
    public string Source { get; set; } = source;
    public RegionalAccessBadgeLedgerStatus Status { get; set; } = status;

    /// <summary>Starts a regional access badge ledger in status planned.</summary>
    public static RegionalAccessBadgeLedger NewRegionalAccessBadgeLedger(string code, string label)
    {
        return new RegionalAccessBadgeLedger(code, label, "", 0, 0, "", "", RegionalAccessBadgeLedgerStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RegionalAccessBadgeLedgerStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RegionalAccessBadgeLedgerStatus next)
    {
        switch (Status)
        {
            case RegionalAccessBadgeLedgerStatus.Planned:
                return next == RegionalAccessBadgeLedgerStatus.Cancelled;
            case RegionalAccessBadgeLedgerStatus.Scheduled:
                return next == RegionalAccessBadgeLedgerStatus.InProgress || next == RegionalAccessBadgeLedgerStatus.Cancelled;
            case RegionalAccessBadgeLedgerStatus.InProgress:
                switch (next)
                {
                    case RegionalAccessBadgeLedgerStatus.Cancelled:
                        return true;
                }
                return false;
            case RegionalAccessBadgeLedgerStatus.Completed:
                switch (next)
                {
                    case RegionalAccessBadgeLedgerStatus.Cancelled:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(RegionalAccessBadgeLedgerStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"regional access badge ledger {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        UpdatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{Code} ({Label})";
    }

    public double NetValue()
    {
        return Math.Round((double)Capacity + (double)UpdatedAt, 2);
    }

    public RegionalAccessBadgeLedger Copy()
    {
        return new RegionalAccessBadgeLedger(Code, Label, Grade, Capacity, UpdatedAt, Location, Source, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Source);
    }
}
