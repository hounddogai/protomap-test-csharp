namespace Acme.Workforce.Assets.LicenseSeat.Agreement;

using Acme.Workforce.Kit.Text;

/// <summary>Where a lenient license seat schedule is in its life cycle.</summary>
public enum LenientLicenseSeatScheduleStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>The record of a lenient license seat schedule and its current status.</summary>
public sealed class LenientLicenseSeatSchedule(string code, string label, int attempts, int priority, int durationMinutes, int sequence, LenientLicenseSeatScheduleStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int Attempts { get; set; } = attempts;
    public int Priority { get; set; } = priority;
    public int DurationMinutes { get; set; } = durationMinutes;
    public int Sequence { get; set; } = sequence;
    public LenientLicenseSeatScheduleStatus Status { get; set; } = status;

    /// <summary>Starts a lenient license seat schedule in status requested.</summary>
    public static LenientLicenseSeatSchedule NewLenientLicenseSeatSchedule(string code, string label)
    {
        return new LenientLicenseSeatSchedule(code, label, 0, 0, 0, 0, LenientLicenseSeatScheduleStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LenientLicenseSeatScheduleStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LenientLicenseSeatScheduleStatus next)
    {
        switch (Status)
        {
            case LenientLicenseSeatScheduleStatus.Requested:
                switch (next)
                {
                    case LenientLicenseSeatScheduleStatus.Granted:
                    case LenientLicenseSeatScheduleStatus.Revoked:
                        return true;
                }
                return false;
            case LenientLicenseSeatScheduleStatus.Granted:
                switch (next)
                {
                    case LenientLicenseSeatScheduleStatus.Revoked:
                    case LenientLicenseSeatScheduleStatus.Expired:
                        return true;
                }
                return false;
            case LenientLicenseSeatScheduleStatus.Revoked:
                return next == LenientLicenseSeatScheduleStatus.Expired;
        }
        return false;
    }

    public void MoveTo(LenientLicenseSeatScheduleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"lenient license seat schedule {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} / {Label}";
    }

    public double WeightedTotal()
    {
        return Math.Round((double)Priority - (double)DurationMinutes, 2);
    }

    public LenientLicenseSeatSchedule Copy()
    {
        return new LenientLicenseSeatSchedule(Code, Label, Attempts, Priority, DurationMinutes, Sequence, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
