namespace Acme.Workforce.Orgdesign.Position.Summary;

using Acme.Workforce.Kit.Text;

/// <summary>Where a adaptive position ledger is in its life cycle.</summary>
public enum AdaptivePositionLedgerStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>One adaptive position ledger, with what the planners need to act on it.</summary>
public sealed class AdaptivePositionLedger(string code, string title, string locale, double budget, string jobCode, int batchSize, int leaveDays, AdaptivePositionLedgerStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Locale { get; set; } = locale;
    public double Budget { get; set; } = budget;
    public string JobCode { get; set; } = jobCode;
    public int BatchSize { get; set; } = batchSize;
    public int LeaveDays { get; set; } = leaveDays;
    public AdaptivePositionLedgerStatus Status { get; set; } = status;

    /// <summary>Starts a adaptive position ledger in status proposed.</summary>
    public static AdaptivePositionLedger NewAdaptivePositionLedger(string code, string title)
    {
        return new AdaptivePositionLedger(code, title, "", 0.0, "", 0, 0, AdaptivePositionLedgerStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AdaptivePositionLedgerStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AdaptivePositionLedgerStatus next)
    {
        switch (Status)
        {
            case AdaptivePositionLedgerStatus.Proposed:
                switch (next)
                {
                    case AdaptivePositionLedgerStatus.Rejected:
                        return true;
                }
                return false;
            case AdaptivePositionLedgerStatus.Approved:
                switch (next)
                {
                    case AdaptivePositionLedgerStatus.Rejected:
                        return true;
                }
                return false;
            case AdaptivePositionLedgerStatus.Rejected:
                switch (next)
                {
                    case AdaptivePositionLedgerStatus.Withdrawn:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(AdaptivePositionLedgerStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"adaptive position ledger {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code}: {Title}";
    }

    public double EffectiveLoad()
    {
        return Math.Round(Budget + (double)BatchSize, 2);
    }

    public AdaptivePositionLedger Copy()
    {
        return new AdaptivePositionLedger(Code, Title, Locale, Budget, JobCode, BatchSize, LeaveDays, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Locale);
    }
}
