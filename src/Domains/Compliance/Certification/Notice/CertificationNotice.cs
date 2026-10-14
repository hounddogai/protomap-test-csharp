namespace Acme.Workforce.Compliance.Certification.Notice;

using Acme.Workforce.Kit.Text;

/// <summary>Where a certification notice is in its life cycle.</summary>
public enum CertificationNoticeStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>A certification notice. Its code is unique within a tenant.</summary>
public sealed class CertificationNotice(string code, string title, string department, double margin, double discount, string timezone, string description, string contactEmail, CertificationNoticeStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Department { get; set; } = department;
    public double Margin { get; set; } = margin;
    public double Discount { get; set; } = discount;
    public string Timezone { get; set; } = timezone;
    public string Description { get; set; } = description;
    public string ContactEmail { get; set; } = contactEmail;
    public CertificationNoticeStatus Status { get; set; } = status;

    /// <summary>Starts a certification notice in status idle.</summary>
    public static CertificationNotice NewCertificationNotice(string code, string title)
    {
        return new CertificationNotice(code, title, "", 0.0, 0.0, "", "", "", CertificationNoticeStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case CertificationNoticeStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(CertificationNoticeStatus next)
    {
        switch (Status)
        {
            case CertificationNoticeStatus.Idle:
                switch (next)
                {
                    case CertificationNoticeStatus.Stopped:
                        return true;
                }
                return false;
            case CertificationNoticeStatus.Warming:
                switch (next)
                {
                    case CertificationNoticeStatus.Ready:
                    case CertificationNoticeStatus.Draining:
                        return true;
                }
                return false;
            case CertificationNoticeStatus.Ready:
                switch (next)
                {
                    case CertificationNoticeStatus.Draining:
                        return true;
                }
                return false;
            case CertificationNoticeStatus.Draining:
                return next == CertificationNoticeStatus.Stopped;
        }
        return false;
    }

    public void MoveTo(CertificationNoticeStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"certification notice {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} / {Title}";
    }

    public double NetVolume()
    {
        return Math.Round(Margin - Discount, 2);
    }

    public CertificationNotice Copy()
    {
        return new CertificationNotice(
            Code,
            Title,
            Department,
            Margin,
            Discount,
            Timezone,
            Description,
            ContactEmail,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Department);
    }
}
