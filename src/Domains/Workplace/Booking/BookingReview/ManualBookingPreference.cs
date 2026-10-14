namespace Acme.Workforce.Workplace.Booking.BookingReview;

using Acme.Workforce.Kit.Text;

/// <summary>Where a manual booking preference is in its life cycle.</summary>
public enum ManualBookingPreferenceStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>A manual booking preference as the booking team keeps it.</summary>
public sealed class ManualBookingPreference(string code, string title, string employeeId, string label, string country, double score, double fte, string location, ManualBookingPreferenceStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string EmployeeId { get; set; } = employeeId;
    public string Label { get; set; } = label;
    public string Country { get; set; } = country;
    public double Score { get; set; } = score;
    public double Fte { get; set; } = fte;
    public string Location { get; set; } = location;
    public ManualBookingPreferenceStatus Status { get; set; } = status;

    /// <summary>Starts a manual booking preference in status created.</summary>
    public static ManualBookingPreference NewManualBookingPreference(string code, string title)
    {
        return new ManualBookingPreference(
            code,
            title,
            "",
            "",
            "",
            0.0,
            0.0,
            "",
            ManualBookingPreferenceStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ManualBookingPreferenceStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ManualBookingPreferenceStatus next)
    {
        switch (Status)
        {
            case ManualBookingPreferenceStatus.Created:
                switch (next)
                {
                    case ManualBookingPreferenceStatus.Validated:
                    case ManualBookingPreferenceStatus.Published:
                        return true;
                }
                return false;
            case ManualBookingPreferenceStatus.Validated:
                switch (next)
                {
                    case ManualBookingPreferenceStatus.Retired:
                        return true;
                }
                return false;
            case ManualBookingPreferenceStatus.Published:
                switch (next)
                {
                    case ManualBookingPreferenceStatus.Retired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(ManualBookingPreferenceStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"manual booking preference {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code}: {Title}";
    }

    public double ProjectedVolume()
    {
        return Math.Round(Score - Fte, 2);
    }

    public ManualBookingPreference Copy()
    {
        return new ManualBookingPreference(Code, Title, EmployeeId, Label, Country, Score, Fte, Location, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Country);
    }
}
