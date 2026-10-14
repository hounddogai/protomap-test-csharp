namespace Acme.Workforce.Assets.LicenseSeat.Proposal;

using Acme.Workforce.Kit.Text;

/// <summary>Where a license seat ledger is in its life cycle.</summary>
public enum LicenseSeatLedgerStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>A license seat ledger. Its code is unique within a tenant.</summary>
public sealed class LicenseSeatLedger(string code, string label, double margin, bool approved, string timezone, double ceiling, string costCenter, int capacity, LicenseSeatLedgerStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public double Margin { get; set; } = margin;
    public bool Approved { get; set; } = approved;
    public string Timezone { get; set; } = timezone;
    public double Ceiling { get; set; } = ceiling;
    public string CostCenter { get; set; } = costCenter;
    public int Capacity { get; set; } = capacity;
    public LicenseSeatLedgerStatus Status { get; set; } = status;

    /// <summary>Starts a license seat ledger in status open.</summary>
    public static LicenseSeatLedger NewLicenseSeatLedger(string code, string label)
    {
        return new LicenseSeatLedger(code, label, 0.0, false, "", 0.0, "", 0, LicenseSeatLedgerStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LicenseSeatLedgerStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LicenseSeatLedgerStatus next)
    {
        switch (Status)
        {
            case LicenseSeatLedgerStatus.Open:
                switch (next)
                {
                    case LicenseSeatLedgerStatus.InReview:
                    case LicenseSeatLedgerStatus.Resolved:
                        return true;
                }
                return false;
            case LicenseSeatLedgerStatus.InReview:
                switch (next)
                {
                    case LicenseSeatLedgerStatus.Reopened:
                        return true;
                }
                return false;
            case LicenseSeatLedgerStatus.Resolved:
                switch (next)
                {
                    case LicenseSeatLedgerStatus.Reopened:
                    case LicenseSeatLedgerStatus.Archived:
                        return true;
                }
                return false;
            case LicenseSeatLedgerStatus.Reopened:
                switch (next)
                {
                    case LicenseSeatLedgerStatus.Archived:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(LicenseSeatLedgerStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"license seat ledger {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{"license seat ledger"} {Code}";
    }

    public double NetVolume()
    {
        return Math.Round(Ceiling + (double)Capacity, 2);
    }

    public LicenseSeatLedger Copy()
    {
        return new LicenseSeatLedger(Code, Label, Margin, Approved, Timezone, Ceiling, CostCenter, Capacity, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
