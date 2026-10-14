namespace Acme.Workforce.Assets.LicenseSeat.Checkpoint;

using Acme.Workforce.Kit.Text;

/// <summary>Where a license seat slot is in its life cycle.</summary>
public enum LicenseSeatSlotStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>One license seat slot, with what the planners need to act on it.</summary>
public sealed class LicenseSeatSlot(string code, string name, string jobCode, int attempts, string notes, string title, LicenseSeatSlotStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string JobCode { get; set; } = jobCode;
    public int Attempts { get; set; } = attempts;
    public string Notes { get; set; } = notes;
    public string Title { get; set; } = title;
    public LicenseSeatSlotStatus Status { get; set; } = status;

    /// <summary>Starts a license seat slot in status draft.</summary>
    public static LicenseSeatSlot NewLicenseSeatSlot(string code, string name)
    {
        return new LicenseSeatSlot(code, name, "", 0, "", "", LicenseSeatSlotStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LicenseSeatSlotStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LicenseSeatSlotStatus next)
    {
        switch (Status)
        {
            case LicenseSeatSlotStatus.Draft:
                switch (next)
                {
                    case LicenseSeatSlotStatus.Closed:
                        return true;
                }
                return false;
            case LicenseSeatSlotStatus.Pending:
                return next == LicenseSeatSlotStatus.Active || next == LicenseSeatSlotStatus.Suspended;
            case LicenseSeatSlotStatus.Active:
                return next == LicenseSeatSlotStatus.Suspended;
            case LicenseSeatSlotStatus.Suspended:
                switch (next)
                {
                    case LicenseSeatSlotStatus.Closed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(LicenseSeatSlotStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"license seat slot {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"license seat slot"} {Code}";
    }

    public double MeasuredValue()
    {
        return (double)Attempts;
    }

    public LicenseSeatSlot Copy()
    {
        return new LicenseSeatSlot(Code, Name, JobCode, Attempts, Notes, Title, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
