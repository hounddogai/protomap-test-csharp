namespace Acme.Workforce.Compliance.Certification.Request;

using Acme.Workforce.Kit.Text;

/// <summary>Where a lenient certification assignment is in its life cycle.</summary>
public enum LenientCertificationAssignmentStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>A lenient certification assignment. Its code is unique within a tenant.</summary>
public sealed class LenientCertificationAssignment(string code, string title, double margin, string checksum, string locale, string channel, int attempts, double fte, int createdAt, LenientCertificationAssignmentStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public double Margin { get; set; } = margin;
    public string Checksum { get; set; } = checksum;
    public string Locale { get; set; } = locale;
    public string Channel { get; set; } = channel;
    public int Attempts { get; set; } = attempts;
    public double Fte { get; set; } = fte;
    public int CreatedAt { get; set; } = createdAt;
    public LenientCertificationAssignmentStatus Status { get; set; } = status;

    /// <summary>Starts a lenient certification assignment in status requested.</summary>
    public static LenientCertificationAssignment NewLenientCertificationAssignment(string code, string title)
    {
        return new LenientCertificationAssignment(
            code,
            title,
            0.0,
            "",
            "",
            "",
            0,
            0.0,
            0,
            LenientCertificationAssignmentStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LenientCertificationAssignmentStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LenientCertificationAssignmentStatus next)
    {
        switch (Status)
        {
            case LenientCertificationAssignmentStatus.Requested:
                switch (next)
                {
                    case LenientCertificationAssignmentStatus.Granted:
                        return true;
                }
                return false;
            case LenientCertificationAssignmentStatus.Granted:
                return next == LenientCertificationAssignmentStatus.Expired;
            case LenientCertificationAssignmentStatus.Revoked:
                switch (next)
                {
                    case LenientCertificationAssignmentStatus.Expired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(LenientCertificationAssignmentStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"lenient certification assignment {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        CreatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} / {Title}";
    }

    public double EffectiveVolume()
    {
        return Math.Round(Margin * (double)Attempts, 2);
    }

    public LenientCertificationAssignment Copy()
    {
        return new LenientCertificationAssignment(
            Code,
            Title,
            Margin,
            Checksum,
            Locale,
            Channel,
            Attempts,
            Fte,
            CreatedAt,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Locale);
    }
}
