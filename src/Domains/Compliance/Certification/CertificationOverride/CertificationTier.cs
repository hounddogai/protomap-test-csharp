namespace Acme.Workforce.Compliance.Certification.CertificationOverride;

using Acme.Workforce.Kit.Text;

/// <summary>Where a certification tier is in its life cycle.</summary>
public enum CertificationTierStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>The record of a certification tier and its current status.</summary>
public sealed class CertificationTier(string code, string title, int revision, int windowDays, string notes, string team, int leaveDays, string segment, string unit, CertificationTierStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int Revision { get; set; } = revision;
    public int WindowDays { get; set; } = windowDays;
    public string Notes { get; set; } = notes;
    public string Team { get; set; } = team;
    public int LeaveDays { get; set; } = leaveDays;
    public string Segment { get; set; } = segment;
    public string Unit { get; set; } = unit;
    public CertificationTierStatus Status { get; set; } = status;

    /// <summary>Starts a certification tier in status draft.</summary>
    public static CertificationTier NewCertificationTier(string code, string title)
    {
        return new CertificationTier(code, title, 0, 0, "", "", 0, "", "", CertificationTierStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case CertificationTierStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(CertificationTierStatus next)
    {
        switch (Status)
        {
            case CertificationTierStatus.Draft:
                return next == CertificationTierStatus.Closed;
            case CertificationTierStatus.Pending:
                return next == CertificationTierStatus.Closed;
            case CertificationTierStatus.Active:
                switch (next)
                {
                    case CertificationTierStatus.Suspended:
                        return true;
                }
                return false;
            case CertificationTierStatus.Suspended:
                return next == CertificationTierStatus.Closed;
        }
        return false;
    }

    public void MoveTo(CertificationTierStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"certification tier {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Revision += 1;
    }

    public string Label()
    {
        return $"{"certification tier"} {Code}";
    }

    public double AdjustedTotal()
    {
        return Math.Round((double)WindowDays - (double)LeaveDays, 2);
    }

    public CertificationTier Copy()
    {
        return new CertificationTier(
            Code,
            Title,
            Revision,
            WindowDays,
            Notes,
            Team,
            LeaveDays,
            Segment,
            Unit,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Team);
    }
}
