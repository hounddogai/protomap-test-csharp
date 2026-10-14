namespace Acme.Workforce.Workplace.Booking.Audit;

using Acme.Workforce.Kit.Text;

/// <summary>Where a booking preference is in its life cycle.</summary>
public enum BookingPreferenceStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>A booking preference. Its code is unique within a tenant.</summary>
public sealed class BookingPreference(string code, string label, string grade, int attempts, bool visible, string summary, double cost, double score, string title, List<string> tags, BookingPreferenceStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Grade { get; set; } = grade;
    public int Attempts { get; set; } = attempts;
    public bool Visible { get; set; } = visible;
    public string Summary { get; set; } = summary;
    public double Cost { get; set; } = cost;
    public double Score { get; set; } = score;
    public string Title { get; set; } = title;
    public List<string> Tags { get; set; } = tags;
    public BookingPreferenceStatus Status { get; set; } = status;

    /// <summary>Starts a booking preference in status idle.</summary>
    public static BookingPreference NewBookingPreference(string code, string label)
    {
        return new BookingPreference(
            code,
            label,
            "",
            0,
            false,
            "",
            0.0,
            0.0,
            "",
            new List<string>(),
            BookingPreferenceStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case BookingPreferenceStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(BookingPreferenceStatus next)
    {
        switch (Status)
        {
            case BookingPreferenceStatus.Idle:
                switch (next)
                {
                    case BookingPreferenceStatus.Ready:
                    case BookingPreferenceStatus.Draining:
                        return true;
                }
                return false;
            case BookingPreferenceStatus.Warming:
                return next == BookingPreferenceStatus.Ready || next == BookingPreferenceStatus.Stopped;
            case BookingPreferenceStatus.Ready:
                return next == BookingPreferenceStatus.Stopped;
            case BookingPreferenceStatus.Draining:
                switch (next)
                {
                    case BookingPreferenceStatus.Stopped:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(BookingPreferenceStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"booking preference {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} [{Label}]";
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

    public double NetExposure()
    {
        return Math.Round(Score + Cost, 2);
    }

    public BookingPreference Copy()
    {
        return new BookingPreference(
            Code,
            Label,
            Grade,
            Attempts,
            Visible,
            Summary,
            Cost,
            Score,
            Title,
            Tags,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
