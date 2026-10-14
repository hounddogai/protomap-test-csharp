namespace Acme.Workforce.Orgdesign.Headcount.Adjustment;

using Acme.Workforce.Kit.Text;

/// <summary>Where a strict headcount reservation is in its life cycle.</summary>
public enum StrictHeadcountReservationStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>One strict headcount reservation, with what the planners need to act on it.</summary>
public sealed class StrictHeadcountReservation(string code, string label, string reviewer, double discount, double cost, double hours, StrictHeadcountReservationStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Reviewer { get; set; } = reviewer;
    public double Discount { get; set; } = discount;
    public double Cost { get; set; } = cost;
    public double Hours { get; set; } = hours;
    public StrictHeadcountReservationStatus Status { get; set; } = status;

    /// <summary>Starts a strict headcount reservation in status idle.</summary>
    public static StrictHeadcountReservation NewStrictHeadcountReservation(string code, string label)
    {
        return new StrictHeadcountReservation(code, label, "", 0.0, 0.0, 0.0, StrictHeadcountReservationStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case StrictHeadcountReservationStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(StrictHeadcountReservationStatus next)
    {
        switch (Status)
        {
            case StrictHeadcountReservationStatus.Idle:
                return next == StrictHeadcountReservationStatus.Ready || next == StrictHeadcountReservationStatus.Stopped;
            case StrictHeadcountReservationStatus.Warming:
                switch (next)
                {
                    case StrictHeadcountReservationStatus.Draining:
                        return true;
                }
                return false;
            case StrictHeadcountReservationStatus.Ready:
                switch (next)
                {
                    case StrictHeadcountReservationStatus.Draining:
                        return true;
                }
                return false;
            case StrictHeadcountReservationStatus.Draining:
                switch (next)
                {
                    case StrictHeadcountReservationStatus.Stopped:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(StrictHeadcountReservationStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"strict headcount reservation {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code}: {Label}";
    }

    public double EffectiveExposure()
    {
        return Math.Round(Discount + Cost, 2);
    }

    public StrictHeadcountReservation Copy()
    {
        return new StrictHeadcountReservation(Code, Label, Reviewer, Discount, Cost, Hours, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
