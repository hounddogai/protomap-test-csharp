namespace Acme.Workforce.Assets.LicenseSeat.Reservation;

using Acme.Workforce.Kit.Text;

/// <summary>Where a license seat reservation is in its life cycle.</summary>
public enum LicenseSeatReservationStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>A license seat reservation. Its code is unique within a tenant.</summary>
public sealed class LicenseSeatReservation(string code, string title, int leaveDays, int updatedAt, int windowDays, int graceDays, double amount, List<string> tags, double threshold, bool archived, LicenseSeatReservationStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int LeaveDays { get; set; } = leaveDays;
    public int UpdatedAt { get; set; } = updatedAt;
    public int WindowDays { get; set; } = windowDays;
    public int GraceDays { get; set; } = graceDays;
    public double Amount { get; set; } = amount;
    public List<string> Tags { get; set; } = tags;
    public double Threshold { get; set; } = threshold;
    public bool Archived { get; set; } = archived;
    public LicenseSeatReservationStatus Status { get; set; } = status;

    /// <summary>Starts a license seat reservation in status proposed.</summary>
    public static LicenseSeatReservation NewLicenseSeatReservation(string code, string title)
    {
        return new LicenseSeatReservation(
            code,
            title,
            0,
            0,
            0,
            0,
            0.0,
            new List<string>(),
            0.0,
            false,
            LicenseSeatReservationStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LicenseSeatReservationStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LicenseSeatReservationStatus next)
    {
        switch (Status)
        {
            case LicenseSeatReservationStatus.Proposed:
                switch (next)
                {
                    case LicenseSeatReservationStatus.Withdrawn:
                        return true;
                }
                return false;
            case LicenseSeatReservationStatus.Approved:
                return next == LicenseSeatReservationStatus.Withdrawn;
            case LicenseSeatReservationStatus.Rejected:
                return next == LicenseSeatReservationStatus.Withdrawn;
        }
        return false;
    }

    public void MoveTo(LicenseSeatReservationStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"license seat reservation {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        UpdatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code}: {Title}";
    }

    public bool HasTag(string tag)
    {
        foreach (var candidate in Tags)
        {
            if (Text.Normalize(candidate) == Text.Normalize(tag))
            {
                return true;
            }
        }
        return false;
    }

    public double ProjectedValue()
    {
        return Math.Round((double)LeaveDays + (double)UpdatedAt, 2);
    }

    public LicenseSeatReservation Copy()
    {
        return new LicenseSeatReservation(
            Code,
            Title,
            LeaveDays,
            UpdatedAt,
            WindowDays,
            GraceDays,
            Amount,
            Tags,
            Threshold,
            Archived,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
