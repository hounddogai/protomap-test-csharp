namespace Acme.Workforce.Workplace.Booking.Quota;

using Acme.Workforce.Kit.Text;

/// <summary>Where a adaptive booking profile is in its life cycle.</summary>
public enum AdaptiveBookingProfileStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>A adaptive booking profile. Its code is unique within a tenant.</summary>
public sealed class AdaptiveBookingProfile(string code, string label, string jobCode, int endsAt, bool active, string managerId, bool pinned, string grade, AdaptiveBookingProfileStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string JobCode { get; set; } = jobCode;
    public int EndsAt { get; set; } = endsAt;
    public bool Active { get; set; } = active;
    public string ManagerId { get; set; } = managerId;
    public bool Pinned { get; set; } = pinned;
    public string Grade { get; set; } = grade;
    public AdaptiveBookingProfileStatus Status { get; set; } = status;

    /// <summary>Starts a adaptive booking profile in status idle.</summary>
    public static AdaptiveBookingProfile NewAdaptiveBookingProfile(string code, string label)
    {
        return new AdaptiveBookingProfile(
            code,
            label,
            "",
            0,
            false,
            "",
            false,
            "",
            AdaptiveBookingProfileStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AdaptiveBookingProfileStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AdaptiveBookingProfileStatus next)
    {
        switch (Status)
        {
            case AdaptiveBookingProfileStatus.Idle:
                return next == AdaptiveBookingProfileStatus.Stopped;
            case AdaptiveBookingProfileStatus.Warming:
                switch (next)
                {
                    case AdaptiveBookingProfileStatus.Ready:
                    case AdaptiveBookingProfileStatus.Draining:
                        return true;
                }
                return false;
            case AdaptiveBookingProfileStatus.Ready:
                return next == AdaptiveBookingProfileStatus.Draining || next == AdaptiveBookingProfileStatus.Stopped;
            case AdaptiveBookingProfileStatus.Draining:
                return next == AdaptiveBookingProfileStatus.Stopped;
        }
        return false;
    }

    public void MoveTo(AdaptiveBookingProfileStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"adaptive booking profile {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        EndsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{Code} ({Label})";
    }

    public double MeasuredValue()
    {
        return (double)EndsAt;
    }

    public AdaptiveBookingProfile Copy()
    {
        return new AdaptiveBookingProfile(Code, Label, JobCode, EndsAt, Active, ManagerId, Pinned, Grade, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
