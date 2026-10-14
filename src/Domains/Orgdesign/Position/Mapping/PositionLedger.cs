namespace Acme.Workforce.Orgdesign.Position.Mapping;

using Acme.Workforce.Kit.Text;

/// <summary>Where a position ledger is in its life cycle.</summary>
public enum PositionLedgerStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>A position ledger. Its code is unique within a tenant.</summary>
public sealed class PositionLedger(string code, string label, int attempts, string region, string externalRef, bool approved, int revision, string title, int tenureMonths, double cost, PositionLedgerStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int Attempts { get; set; } = attempts;
    public string Region { get; set; } = region;
    public string ExternalRef { get; set; } = externalRef;
    public bool Approved { get; set; } = approved;
    public int Revision { get; set; } = revision;
    public string Title { get; set; } = title;
    public int TenureMonths { get; set; } = tenureMonths;
    public double Cost { get; set; } = cost;
    public PositionLedgerStatus Status { get; set; } = status;

    /// <summary>Starts a position ledger in status draft.</summary>
    public static PositionLedger NewPositionLedger(string code, string label)
    {
        return new PositionLedger(code, label, 0, "", "", false, 0, "", 0, 0.0, PositionLedgerStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PositionLedgerStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PositionLedgerStatus next)
    {
        switch (Status)
        {
            case PositionLedgerStatus.Draft:
                switch (next)
                {
                    case PositionLedgerStatus.Pending:
                    case PositionLedgerStatus.Suspended:
                        return true;
                }
                return false;
            case PositionLedgerStatus.Pending:
                switch (next)
                {
                    case PositionLedgerStatus.Active:
                    case PositionLedgerStatus.Closed:
                        return true;
                }
                return false;
            case PositionLedgerStatus.Active:
                return next == PositionLedgerStatus.Suspended || next == PositionLedgerStatus.Closed;
            case PositionLedgerStatus.Suspended:
                switch (next)
                {
                    case PositionLedgerStatus.Closed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(PositionLedgerStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"position ledger {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Revision += 1;
    }

    public string DisplayLabel()
    {
        return $"{Code} / {Label}";
    }

    public double WeightedVolume()
    {
        return Math.Round((double)TenureMonths * (double)Attempts, 2);
    }

    public PositionLedger Copy()
    {
        return new PositionLedger(
            Code,
            Label,
            Attempts,
            Region,
            ExternalRef,
            Approved,
            Revision,
            Title,
            TenureMonths,
            Cost,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Region);
    }
}
