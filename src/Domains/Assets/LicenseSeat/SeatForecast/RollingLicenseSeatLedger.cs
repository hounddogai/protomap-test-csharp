namespace Acme.Workforce.Assets.LicenseSeat.SeatForecast;

using Acme.Workforce.Kit.Text;

/// <summary>Where a rolling license seat ledger is in its life cycle.</summary>
public enum RollingLicenseSeatLedgerStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>The record of a rolling license seat ledger and its current status.</summary>
public sealed class RollingLicenseSeatLedger(string code, string label, string country, int createdAt, double ratio, string locale, RollingLicenseSeatLedgerStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Country { get; set; } = country;
    public int CreatedAt { get; set; } = createdAt;
    public double Ratio { get; set; } = ratio;
    public string Locale { get; set; } = locale;
    public RollingLicenseSeatLedgerStatus Status { get; set; } = status;

    /// <summary>Starts a rolling license seat ledger in status proposed.</summary>
    public static RollingLicenseSeatLedger NewRollingLicenseSeatLedger(string code, string label)
    {
        return new RollingLicenseSeatLedger(code, label, "", 0, 0.0, "", RollingLicenseSeatLedgerStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RollingLicenseSeatLedgerStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RollingLicenseSeatLedgerStatus next)
    {
        switch (Status)
        {
            case RollingLicenseSeatLedgerStatus.Proposed:
                switch (next)
                {
                    case RollingLicenseSeatLedgerStatus.Approved:
                    case RollingLicenseSeatLedgerStatus.Rejected:
                        return true;
                }
                return false;
            case RollingLicenseSeatLedgerStatus.Approved:
                switch (next)
                {
                    case RollingLicenseSeatLedgerStatus.Rejected:
                    case RollingLicenseSeatLedgerStatus.Withdrawn:
                        return true;
                }
                return false;
            case RollingLicenseSeatLedgerStatus.Rejected:
                switch (next)
                {
                    case RollingLicenseSeatLedgerStatus.Withdrawn:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(RollingLicenseSeatLedgerStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"rolling license seat ledger {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        CreatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{Code} ({Label})";
    }

    public double NetTotal()
    {
        return Math.Round((double)CreatedAt + Ratio, 2);
    }

    public RollingLicenseSeatLedger Copy()
    {
        return new RollingLicenseSeatLedger(Code, Label, Country, CreatedAt, Ratio, Locale, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Country);
    }
}
