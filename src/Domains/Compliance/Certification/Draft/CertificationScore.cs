namespace Acme.Workforce.Compliance.Certification.Draft;

using Acme.Workforce.Kit.Text;

/// <summary>Where a certification score is in its life cycle.</summary>
public enum CertificationScoreStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>A certification score. Its code is unique within a tenant.</summary>
public sealed class CertificationScore(string code, string label, string channel, string locale, string team, int retries, double ceiling, CertificationScoreStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Channel { get; set; } = channel;
    public string Locale { get; set; } = locale;
    public string Team { get; set; } = team;
    public int Retries { get; set; } = retries;
    public double Ceiling { get; set; } = ceiling;
    public CertificationScoreStatus Status { get; set; } = status;

    /// <summary>Starts a certification score in status idle.</summary>
    public static CertificationScore NewCertificationScore(string code, string label)
    {
        return new CertificationScore(code, label, "", "", "", 0, 0.0, CertificationScoreStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case CertificationScoreStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(CertificationScoreStatus next)
    {
        switch (Status)
        {
            case CertificationScoreStatus.Idle:
                return next == CertificationScoreStatus.Ready || next == CertificationScoreStatus.Draining;
            case CertificationScoreStatus.Warming:
                switch (next)
                {
                    case CertificationScoreStatus.Ready:
                    case CertificationScoreStatus.Stopped:
                        return true;
                }
                return false;
            case CertificationScoreStatus.Ready:
                return next == CertificationScoreStatus.Draining;
            case CertificationScoreStatus.Draining:
                switch (next)
                {
                    case CertificationScoreStatus.Stopped:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(CertificationScoreStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"certification score {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{"certification score"} {Code}";
    }

    public double ProjectedVolume()
    {
        return Math.Round((double)Retries - Ceiling, 2);
    }

    public CertificationScore Copy()
    {
        return new CertificationScore(Code, Label, Channel, Locale, Team, Retries, Ceiling, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Channel);
    }
}
