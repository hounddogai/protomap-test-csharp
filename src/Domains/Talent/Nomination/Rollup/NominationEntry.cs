namespace Acme.Workforce.Talent.Nomination.Rollup;

using Acme.Workforce.Kit.Text;

/// <summary>Where a nomination entry is in its life cycle.</summary>
public enum NominationEntryStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>The record of a nomination entry and its current status.</summary>
public sealed class NominationEntry(string code, string name, bool approved, string reviewer, double floorValue, string department, double hours, NominationEntryStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public bool Approved { get; set; } = approved;
    public string Reviewer { get; set; } = reviewer;
    public double FloorValue { get; set; } = floorValue;
    public string Department { get; set; } = department;
    public double Hours { get; set; } = hours;
    public NominationEntryStatus Status { get; set; } = status;

    /// <summary>Starts a nomination entry in status requested.</summary>
    public static NominationEntry NewNominationEntry(string code, string name)
    {
        return new NominationEntry(code, name, false, "", 0.0, "", 0.0, NominationEntryStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case NominationEntryStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(NominationEntryStatus next)
    {
        switch (Status)
        {
            case NominationEntryStatus.Requested:
                switch (next)
                {
                    case NominationEntryStatus.Granted:
                        return true;
                }
                return false;
            case NominationEntryStatus.Granted:
                return next == NominationEntryStatus.Revoked || next == NominationEntryStatus.Expired;
            case NominationEntryStatus.Revoked:
                switch (next)
                {
                    case NominationEntryStatus.Expired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(NominationEntryStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"nomination entry {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} / {Name}";
    }

    public double NetTotal()
    {
        return Math.Round(FloorValue + Hours, 2);
    }

    public NominationEntry Copy()
    {
        return new NominationEntry(Code, Name, Approved, Reviewer, FloorValue, Department, Hours, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Department);
    }
}
