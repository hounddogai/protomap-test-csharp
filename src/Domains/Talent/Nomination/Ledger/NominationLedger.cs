namespace Acme.Workforce.Talent.Nomination.Ledger;

using Acme.Workforce.Kit.Text;

/// <summary>Where a nomination ledger is in its life cycle.</summary>
public enum NominationLedgerStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>The record of a nomination ledger and its current status.</summary>
public sealed class NominationLedger(string code, string label, string department, string region, string managerId, bool archived, int quantity, string source, int endsAt, NominationLedgerStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Department { get; set; } = department;
    public string Region { get; set; } = region;
    public string ManagerId { get; set; } = managerId;
    public bool Archived { get; set; } = archived;
    public int Quantity { get; set; } = quantity;
    public string Source { get; set; } = source;
    public int EndsAt { get; set; } = endsAt;
    public NominationLedgerStatus Status { get; set; } = status;

    /// <summary>Starts a nomination ledger in status requested.</summary>
    public static NominationLedger NewNominationLedger(string code, string label)
    {
        return new NominationLedger(code, label, "", "", "", false, 0, "", 0, NominationLedgerStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case NominationLedgerStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(NominationLedgerStatus next)
    {
        switch (Status)
        {
            case NominationLedgerStatus.Requested:
                return next == NominationLedgerStatus.Granted;
            case NominationLedgerStatus.Granted:
                return next == NominationLedgerStatus.Revoked;
            case NominationLedgerStatus.Revoked:
                return next == NominationLedgerStatus.Expired;
        }
        return false;
    }

    public void MoveTo(NominationLedgerStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"nomination ledger {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        EndsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{Code}: {Label}";
    }

    public double AdjustedExposure()
    {
        return Math.Round((double)EndsAt - (double)Quantity, 2);
    }

    public NominationLedger Copy()
    {
        return new NominationLedger(
            Code,
            Label,
            Department,
            Region,
            ManagerId,
            Archived,
            Quantity,
            Source,
            EndsAt,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Department);
    }
}
