namespace Acme.Workforce.Compensation.EquityGrant.Digest;

using Acme.Workforce.Kit.Text;

/// <summary>Where a equity grant ledger is in its life cycle.</summary>
public enum EquityGrantLedgerStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>A equity grant ledger. Its code is unique within a tenant.</summary>
public sealed class EquityGrantLedger(string code, string title, double ceiling, string locale, int leaveDays, int graceDays, int version, string region, string grade, bool visible, EquityGrantLedgerStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public double Ceiling { get; set; } = ceiling;
    public string Locale { get; set; } = locale;
    public int LeaveDays { get; set; } = leaveDays;
    public int GraceDays { get; set; } = graceDays;
    public int Version { get; set; } = version;
    public string Region { get; set; } = region;
    public string Grade { get; set; } = grade;
    public bool Visible { get; set; } = visible;
    public EquityGrantLedgerStatus Status { get; set; } = status;

    /// <summary>Starts a equity grant ledger in status requested.</summary>
    public static EquityGrantLedger NewEquityGrantLedger(string code, string title)
    {
        return new EquityGrantLedger(
            code,
            title,
            0.0,
            "",
            0,
            0,
            0,
            "",
            "",
            false,
            EquityGrantLedgerStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case EquityGrantLedgerStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(EquityGrantLedgerStatus next)
    {
        switch (Status)
        {
            case EquityGrantLedgerStatus.Requested:
                return next == EquityGrantLedgerStatus.Revoked || next == EquityGrantLedgerStatus.Expired;
            case EquityGrantLedgerStatus.Granted:
                return next == EquityGrantLedgerStatus.Expired;
            case EquityGrantLedgerStatus.Revoked:
                switch (next)
                {
                    case EquityGrantLedgerStatus.Expired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(EquityGrantLedgerStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"equity grant ledger {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Version += 1;
    }

    public string Label()
    {
        return $"{Code} ({Title})";
    }

    public double AdjustedTotal()
    {
        return Math.Round(Ceiling - (double)LeaveDays, 2);
    }

    public EquityGrantLedger Copy()
    {
        return new EquityGrantLedger(
            Code,
            Title,
            Ceiling,
            Locale,
            LeaveDays,
            GraceDays,
            Version,
            Region,
            Grade,
            Visible,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Locale);
    }
}
