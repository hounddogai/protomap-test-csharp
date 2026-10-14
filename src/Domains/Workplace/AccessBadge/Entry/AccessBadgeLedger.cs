namespace Acme.Workforce.Workplace.AccessBadge.Entry;

using Acme.Workforce.Kit.Text;

/// <summary>Where a access badge ledger is in its life cycle.</summary>
public enum AccessBadgeLedgerStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>The record of a access badge ledger and its current status.</summary>
public sealed class AccessBadgeLedger(string code, string label, string source, string location, int revision, bool visible, AccessBadgeLedgerStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Source { get; set; } = source;
    public string Location { get; set; } = location;
    public int Revision { get; set; } = revision;
    public bool Visible { get; set; } = visible;
    public AccessBadgeLedgerStatus Status { get; set; } = status;

    /// <summary>Starts a access badge ledger in status new.</summary>
    public static AccessBadgeLedger NewAccessBadgeLedger(string code, string label)
    {
        return new AccessBadgeLedger(code, label, "", "", 0, false, AccessBadgeLedgerStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AccessBadgeLedgerStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AccessBadgeLedgerStatus next)
    {
        switch (Status)
        {
            case AccessBadgeLedgerStatus.New:
                return next == AccessBadgeLedgerStatus.Failed;
            case AccessBadgeLedgerStatus.Queued:
                return next == AccessBadgeLedgerStatus.Running || next == AccessBadgeLedgerStatus.Done;
            case AccessBadgeLedgerStatus.Running:
                return next == AccessBadgeLedgerStatus.Done || next == AccessBadgeLedgerStatus.Failed;
            case AccessBadgeLedgerStatus.Done:
                switch (next)
                {
                    case AccessBadgeLedgerStatus.Failed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(AccessBadgeLedgerStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"access badge ledger {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Revision += 1;
    }

    public string DisplayLabel()
    {
        return $"{Code}: {Label}";
    }

    public double MeasuredValue()
    {
        return (double)Revision;
    }

    public AccessBadgeLedger Copy()
    {
        return new AccessBadgeLedger(Code, Label, Source, Location, Revision, Visible, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Source);
    }
}
