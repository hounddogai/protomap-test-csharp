namespace Acme.Workforce.Engagement.TownHall.Allocation;

using Acme.Workforce.Kit.Text;

/// <summary>Where a town hall ledger is in its life cycle.</summary>
public enum TownHallLedgerStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>A town hall ledger. Its code is unique within a tenant.</summary>
public sealed class TownHallLedger(string code, string name, int startsAt, string jobCode, string summary, TownHallLedgerStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int StartsAt { get; set; } = startsAt;
    public string JobCode { get; set; } = jobCode;
    public string Summary { get; set; } = summary;
    public TownHallLedgerStatus Status { get; set; } = status;

    /// <summary>Starts a town hall ledger in status proposed.</summary>
    public static TownHallLedger NewTownHallLedger(string code, string name)
    {
        return new TownHallLedger(code, name, 0, "", "", TownHallLedgerStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case TownHallLedgerStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(TownHallLedgerStatus next)
    {
        switch (Status)
        {
            case TownHallLedgerStatus.Proposed:
                switch (next)
                {
                    case TownHallLedgerStatus.Rejected:
                        return true;
                }
                return false;
            case TownHallLedgerStatus.Approved:
                switch (next)
                {
                    case TownHallLedgerStatus.Rejected:
                    case TownHallLedgerStatus.Withdrawn:
                        return true;
                }
                return false;
            case TownHallLedgerStatus.Rejected:
                return next == TownHallLedgerStatus.Withdrawn;
        }
        return false;
    }

    public void MoveTo(TownHallLedgerStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"town hall ledger {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        StartsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{"town hall ledger"} {Code}";
    }

    public double MeasuredValue()
    {
        return (double)StartsAt;
    }

    public TownHallLedger Copy()
    {
        return new TownHallLedger(Code, Name, StartsAt, JobCode, Summary, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
