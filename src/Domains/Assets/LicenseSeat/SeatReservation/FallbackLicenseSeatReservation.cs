namespace Acme.Workforce.Assets.LicenseSeat.SeatReservation;

using Acme.Workforce.Kit.Text;

/// <summary>Where a fallback license seat reservation is in its life cycle.</summary>
public enum FallbackLicenseSeatReservationStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>A fallback license seat reservation as the license seat team keeps it.</summary>
public sealed class FallbackLicenseSeatReservation(string code, string name, string title, double margin, double weight, int retries, string timezone, string jobCode, List<string> tags, FallbackLicenseSeatReservationStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Title { get; set; } = title;
    public double Margin { get; set; } = margin;
    public double Weight { get; set; } = weight;
    public int Retries { get; set; } = retries;
    public string Timezone { get; set; } = timezone;
    public string JobCode { get; set; } = jobCode;
    public List<string> Tags { get; set; } = tags;
    public FallbackLicenseSeatReservationStatus Status { get; set; } = status;

    /// <summary>Starts a fallback license seat reservation in status proposed.</summary>
    public static FallbackLicenseSeatReservation NewFallbackLicenseSeatReservation(string code, string name)
    {
        return new FallbackLicenseSeatReservation(
            code,
            name,
            "",
            0.0,
            0.0,
            0,
            "",
            "",
            new List<string>(),
            FallbackLicenseSeatReservationStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case FallbackLicenseSeatReservationStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(FallbackLicenseSeatReservationStatus next)
    {
        switch (Status)
        {
            case FallbackLicenseSeatReservationStatus.Proposed:
                switch (next)
                {
                    case FallbackLicenseSeatReservationStatus.Approved:
                    case FallbackLicenseSeatReservationStatus.Withdrawn:
                        return true;
                }
                return false;
            case FallbackLicenseSeatReservationStatus.Approved:
                return next == FallbackLicenseSeatReservationStatus.Withdrawn;
            case FallbackLicenseSeatReservationStatus.Rejected:
                switch (next)
                {
                    case FallbackLicenseSeatReservationStatus.Withdrawn:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(FallbackLicenseSeatReservationStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"fallback license seat reservation {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} ({Name})";
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

    public double WeightedVolume()
    {
        return Math.Round((double)Retries + Margin, 2);
    }

    public FallbackLicenseSeatReservation Copy()
    {
        return new FallbackLicenseSeatReservation(
            Code,
            Name,
            Title,
            Margin,
            Weight,
            Retries,
            Timezone,
            JobCode,
            Tags,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
