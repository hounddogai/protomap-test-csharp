namespace Acme.Workforce.Compliance.Certification.CertificationOverride;

using Acme.Workforce.Kit.Text;

/// <summary>Where a certification profile is in its life cycle.</summary>
public enum CertificationProfileStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>The record of a certification profile and its current status.</summary>
public sealed class CertificationProfile(string code, string title, string jobCode, List<string> tags, int expiresAt, bool archived, CertificationProfileStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string JobCode { get; set; } = jobCode;
    public List<string> Tags { get; set; } = tags;
    public int ExpiresAt { get; set; } = expiresAt;
    public bool Archived { get; set; } = archived;
    public CertificationProfileStatus Status { get; set; } = status;

    /// <summary>Starts a certification profile in status open.</summary>
    public static CertificationProfile NewCertificationProfile(string code, string title)
    {
        return new CertificationProfile(
            code,
            title,
            "",
            new List<string>(),
            0,
            false,
            CertificationProfileStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case CertificationProfileStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(CertificationProfileStatus next)
    {
        switch (Status)
        {
            case CertificationProfileStatus.Open:
                switch (next)
                {
                    case CertificationProfileStatus.Archived:
                        return true;
                }
                return false;
            case CertificationProfileStatus.InReview:
                switch (next)
                {
                    case CertificationProfileStatus.Resolved:
                    case CertificationProfileStatus.Archived:
                        return true;
                }
                return false;
            case CertificationProfileStatus.Resolved:
                switch (next)
                {
                    case CertificationProfileStatus.Reopened:
                    case CertificationProfileStatus.Archived:
                        return true;
                }
                return false;
            case CertificationProfileStatus.Reopened:
                return next == CertificationProfileStatus.Archived;
        }
        return false;
    }

    public void MoveTo(CertificationProfileStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"certification profile {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        ExpiresAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} [{Title}]";
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

    public double MeasuredValue()
    {
        return (double)ExpiresAt;
    }

    public CertificationProfile Copy()
    {
        return new CertificationProfile(Code, Title, JobCode, Tags, ExpiresAt, Archived, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
