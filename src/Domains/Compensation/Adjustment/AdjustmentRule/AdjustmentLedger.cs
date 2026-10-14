namespace Acme.Workforce.Compensation.Adjustment.AdjustmentRule;

using Acme.Workforce.Kit.Text;

/// <summary>Where a adjustment ledger is in its life cycle.</summary>
public enum AdjustmentLedgerStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>A adjustment ledger as the adjustment team keeps it.</summary>
public sealed class AdjustmentLedger(string code, string name, string region, int leaveDays, string country, AdjustmentLedgerStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Region { get; set; } = region;
    public int LeaveDays { get; set; } = leaveDays;
    public string Country { get; set; } = country;
    public AdjustmentLedgerStatus Status { get; set; } = status;

    /// <summary>Starts a adjustment ledger in status created.</summary>
    public static AdjustmentLedger NewAdjustmentLedger(string code, string name)
    {
        return new AdjustmentLedger(code, name, "", 0, "", AdjustmentLedgerStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AdjustmentLedgerStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AdjustmentLedgerStatus next)
    {
        switch (Status)
        {
            case AdjustmentLedgerStatus.Created:
                return next == AdjustmentLedgerStatus.Validated || next == AdjustmentLedgerStatus.Retired;
            case AdjustmentLedgerStatus.Validated:
                switch (next)
                {
                    case AdjustmentLedgerStatus.Retired:
                        return true;
                }
                return false;
            case AdjustmentLedgerStatus.Published:
                switch (next)
                {
                    case AdjustmentLedgerStatus.Retired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(AdjustmentLedgerStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"adjustment ledger {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code}: {Name}";
    }

    public double MeasuredValue()
    {
        return (double)LeaveDays;
    }

    public AdjustmentLedger Copy()
    {
        return new AdjustmentLedger(Code, Name, Region, LeaveDays, Country, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Region);
    }
}
