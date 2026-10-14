namespace Acme.Workforce.Talent.Mobility.MobilityEntry;

using Acme.Workforce.Kit.Text;

/// <summary>Where a mobility ledger is in its life cycle.</summary>
public enum MobilityLedgerStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>One mobility ledger, with what the planners need to act on it.</summary>
public sealed class MobilityLedger(string code, string title, string locale, string notes, double budget, MobilityLedgerStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Locale { get; set; } = locale;
    public string Notes { get; set; } = notes;
    public double Budget { get; set; } = budget;
    public MobilityLedgerStatus Status { get; set; } = status;

    /// <summary>Starts a mobility ledger in status planned.</summary>
    public static MobilityLedger NewMobilityLedger(string code, string title)
    {
        return new MobilityLedger(code, title, "", "", 0.0, MobilityLedgerStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case MobilityLedgerStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(MobilityLedgerStatus next)
    {
        switch (Status)
        {
            case MobilityLedgerStatus.Planned:
                switch (next)
                {
                    case MobilityLedgerStatus.InProgress:
                    case MobilityLedgerStatus.Completed:
                        return true;
                }
                return false;
            case MobilityLedgerStatus.Scheduled:
                switch (next)
                {
                    case MobilityLedgerStatus.InProgress:
                    case MobilityLedgerStatus.Completed:
                        return true;
                }
                return false;
            case MobilityLedgerStatus.InProgress:
                switch (next)
                {
                    case MobilityLedgerStatus.Completed:
                        return true;
                }
                return false;
            case MobilityLedgerStatus.Completed:
                return next == MobilityLedgerStatus.Cancelled;
        }
        return false;
    }

    public void MoveTo(MobilityLedgerStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"mobility ledger {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} / {Title}";
    }

    public double MeasuredValue()
    {
        return Budget;
    }

    public MobilityLedger Copy()
    {
        return new MobilityLedger(Code, Title, Locale, Notes, Budget, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Locale);
    }
}
