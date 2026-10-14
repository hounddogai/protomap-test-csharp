namespace Acme.Workforce.Talent.Nomination.Adjustment;

using Acme.Workforce.Kit.Text;

/// <summary>Where a seasonal nomination ledger is in its life cycle.</summary>
public enum SeasonalNominationLedgerStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>The record of a seasonal nomination ledger and its current status.</summary>
public sealed class SeasonalNominationLedger(string code, string label, int expiresAt, double budget, double score, SeasonalNominationLedgerStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int ExpiresAt { get; set; } = expiresAt;
    public double Budget { get; set; } = budget;
    public double Score { get; set; } = score;
    public SeasonalNominationLedgerStatus Status { get; set; } = status;

    /// <summary>Starts a seasonal nomination ledger in status draft.</summary>
    public static SeasonalNominationLedger NewSeasonalNominationLedger(string code, string label)
    {
        return new SeasonalNominationLedger(code, label, 0, 0.0, 0.0, SeasonalNominationLedgerStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case SeasonalNominationLedgerStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(SeasonalNominationLedgerStatus next)
    {
        switch (Status)
        {
            case SeasonalNominationLedgerStatus.Draft:
                return next == SeasonalNominationLedgerStatus.Pending || next == SeasonalNominationLedgerStatus.Closed;
            case SeasonalNominationLedgerStatus.Pending:
                return next == SeasonalNominationLedgerStatus.Active || next == SeasonalNominationLedgerStatus.Suspended;
            case SeasonalNominationLedgerStatus.Active:
                return next == SeasonalNominationLedgerStatus.Suspended || next == SeasonalNominationLedgerStatus.Closed;
            case SeasonalNominationLedgerStatus.Suspended:
                return next == SeasonalNominationLedgerStatus.Closed;
        }
        return false;
    }

    public void MoveTo(SeasonalNominationLedgerStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"seasonal nomination ledger {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        ExpiresAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{Code} ({Label})";
    }

    public double EffectiveExposure()
    {
        return Math.Round((double)ExpiresAt - Budget, 2);
    }

    public SeasonalNominationLedger Copy()
    {
        return new SeasonalNominationLedger(Code, Label, ExpiresAt, Budget, Score, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
