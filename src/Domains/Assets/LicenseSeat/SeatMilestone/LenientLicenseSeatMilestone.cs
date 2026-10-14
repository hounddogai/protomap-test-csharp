namespace Acme.Workforce.Assets.LicenseSeat.SeatMilestone;

using Acme.Workforce.Kit.Text;

/// <summary>Where a lenient license seat milestone is in its life cycle.</summary>
public enum LenientLicenseSeatMilestoneStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>The record of a lenient license seat milestone and its current status.</summary>
public sealed class LenientLicenseSeatMilestone(string code, string name, int attempts, string timezone, bool approved, LenientLicenseSeatMilestoneStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int Attempts { get; set; } = attempts;
    public string Timezone { get; set; } = timezone;
    public bool Approved { get; set; } = approved;
    public LenientLicenseSeatMilestoneStatus Status { get; set; } = status;

    /// <summary>Starts a lenient license seat milestone in status requested.</summary>
    public static LenientLicenseSeatMilestone NewLenientLicenseSeatMilestone(string code, string name)
    {
        return new LenientLicenseSeatMilestone(
            code,
            name,
            0,
            "",
            false,
            LenientLicenseSeatMilestoneStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LenientLicenseSeatMilestoneStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LenientLicenseSeatMilestoneStatus next)
    {
        switch (Status)
        {
            case LenientLicenseSeatMilestoneStatus.Requested:
                return next == LenientLicenseSeatMilestoneStatus.Granted || next == LenientLicenseSeatMilestoneStatus.Expired;
            case LenientLicenseSeatMilestoneStatus.Granted:
                return next == LenientLicenseSeatMilestoneStatus.Revoked || next == LenientLicenseSeatMilestoneStatus.Expired;
            case LenientLicenseSeatMilestoneStatus.Revoked:
                switch (next)
                {
                    case LenientLicenseSeatMilestoneStatus.Expired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(LenientLicenseSeatMilestoneStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"lenient license seat milestone {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} / {Name}";
    }

    public double MeasuredValue()
    {
        return (double)Attempts;
    }

    public LenientLicenseSeatMilestone Copy()
    {
        return new LenientLicenseSeatMilestone(Code, Name, Attempts, Timezone, Approved, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
