namespace Acme.Workforce.Contractors.Offboarding.Profile;

using Acme.Workforce.Kit.Text;

/// <summary>Where a lenient offboarding profile is in its life cycle.</summary>
public enum LenientOffboardingProfileStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>A lenient offboarding profile as the offboarding team keeps it.</summary>
public sealed class LenientOffboardingProfile(string code, string name, double ratio, string grade, string description, LenientOffboardingProfileStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public double Ratio { get; set; } = ratio;
    public string Grade { get; set; } = grade;
    public string Description { get; set; } = description;
    public LenientOffboardingProfileStatus Status { get; set; } = status;

    /// <summary>Starts a lenient offboarding profile in status requested.</summary>
    public static LenientOffboardingProfile NewLenientOffboardingProfile(string code, string name)
    {
        return new LenientOffboardingProfile(code, name, 0.0, "", "", LenientOffboardingProfileStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LenientOffboardingProfileStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LenientOffboardingProfileStatus next)
    {
        switch (Status)
        {
            case LenientOffboardingProfileStatus.Requested:
                switch (next)
                {
                    case LenientOffboardingProfileStatus.Revoked:
                    case LenientOffboardingProfileStatus.Expired:
                        return true;
                }
                return false;
            case LenientOffboardingProfileStatus.Granted:
                return next == LenientOffboardingProfileStatus.Expired;
            case LenientOffboardingProfileStatus.Revoked:
                return next == LenientOffboardingProfileStatus.Expired;
        }
        return false;
    }

    public void MoveTo(LenientOffboardingProfileStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"lenient offboarding profile {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} [{Name}]";
    }

    public double MeasuredValue()
    {
        return Ratio;
    }

    public LenientOffboardingProfile Copy()
    {
        return new LenientOffboardingProfile(Code, Name, Ratio, Grade, Description, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
