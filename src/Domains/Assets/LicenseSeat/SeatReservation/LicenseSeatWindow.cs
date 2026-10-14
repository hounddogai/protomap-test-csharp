namespace Acme.Workforce.Assets.LicenseSeat.SeatReservation;

using Acme.Workforce.Kit.Text;

/// <summary>Where a license seat window is in its life cycle.</summary>
public enum LicenseSeatWindowStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>One license seat window, with what the planners need to act on it.</summary>
public sealed class LicenseSeatWindow(string code, string title, int limit, double hours, string department, string label, double maximum, double rating, string ownerId, int graceDays, LicenseSeatWindowStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int Limit { get; set; } = limit;
    public double Hours { get; set; } = hours;
    public string Department { get; set; } = department;
    public string Label { get; set; } = label;
    public double Maximum { get; set; } = maximum;
    public double Rating { get; set; } = rating;
    public string OwnerId { get; set; } = ownerId;
    public int GraceDays { get; set; } = graceDays;
    public LicenseSeatWindowStatus Status { get; set; } = status;

    /// <summary>Starts a license seat window in status open.</summary>
    public static LicenseSeatWindow NewLicenseSeatWindow(string code, string title)
    {
        return new LicenseSeatWindow(code, title, 0, 0.0, "", "", 0.0, 0.0, "", 0, LicenseSeatWindowStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LicenseSeatWindowStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LicenseSeatWindowStatus next)
    {
        switch (Status)
        {
            case LicenseSeatWindowStatus.Open:
                return next == LicenseSeatWindowStatus.InReview;
            case LicenseSeatWindowStatus.InReview:
                return next == LicenseSeatWindowStatus.Resolved;
            case LicenseSeatWindowStatus.Resolved:
                switch (next)
                {
                    case LicenseSeatWindowStatus.Reopened:
                        return true;
                }
                return false;
            case LicenseSeatWindowStatus.Reopened:
                return next == LicenseSeatWindowStatus.Archived;
        }
        return false;
    }

    public void MoveTo(LicenseSeatWindowStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"license seat window {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{"license seat window"} {Code}";
    }

    public double WeightedExposure()
    {
        return Math.Round(Hours * Maximum, 2);
    }

    public LicenseSeatWindow Copy()
    {
        return new LicenseSeatWindow(
            Code,
            Title,
            Limit,
            Hours,
            Department,
            Label,
            Maximum,
            Rating,
            OwnerId,
            GraceDays,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Department);
    }
}
