namespace Acme.Workforce.Compliance.Certification.Manifest;

using Acme.Workforce.Kit.Text;

/// <summary>Where a certification digest is in its life cycle.</summary>
public enum CertificationDigestStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>The record of a certification digest and its current status.</summary>
public sealed class CertificationDigest(string code, string label, int startsAt, string phone, bool active, double ceiling, double score, double cost, CertificationDigestStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int StartsAt { get; set; } = startsAt;
    public string Phone { get; set; } = phone;
    public bool Active { get; set; } = active;
    public double Ceiling { get; set; } = ceiling;
    public double Score { get; set; } = score;
    public double Cost { get; set; } = cost;
    public CertificationDigestStatus Status { get; set; } = status;

    /// <summary>Starts a certification digest in status draft.</summary>
    public static CertificationDigest NewCertificationDigest(string code, string label)
    {
        return new CertificationDigest(code, label, 0, "", false, 0.0, 0.0, 0.0, CertificationDigestStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case CertificationDigestStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(CertificationDigestStatus next)
    {
        switch (Status)
        {
            case CertificationDigestStatus.Draft:
                switch (next)
                {
                    case CertificationDigestStatus.Pending:
                    case CertificationDigestStatus.Active:
                        return true;
                }
                return false;
            case CertificationDigestStatus.Pending:
                switch (next)
                {
                    case CertificationDigestStatus.Suspended:
                        return true;
                }
                return false;
            case CertificationDigestStatus.Active:
                return next == CertificationDigestStatus.Suspended;
            case CertificationDigestStatus.Suspended:
                switch (next)
                {
                    case CertificationDigestStatus.Closed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(CertificationDigestStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"certification digest {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        StartsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{Code}: {Label}";
    }

    public double NetExposure()
    {
        return Math.Round((double)StartsAt - Score, 2);
    }

    public CertificationDigest Copy()
    {
        return new CertificationDigest(Code, Label, StartsAt, Phone, Active, Ceiling, Score, Cost, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
