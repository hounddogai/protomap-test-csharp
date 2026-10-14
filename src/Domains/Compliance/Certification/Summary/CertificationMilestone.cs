namespace Acme.Workforce.Compliance.Certification.Summary;

using Acme.Workforce.Kit.Text;

/// <summary>Where a certification milestone is in its life cycle.</summary>
public enum CertificationMilestoneStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>One certification milestone, with what the planners need to act on it.</summary>
public sealed class CertificationMilestone(string code, string title, string salaryBand, string source, string ownerId, int endsAt, double rate, double minimum, string checksum, CertificationMilestoneStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string SalaryBand { get; set; } = salaryBand;
    public string Source { get; set; } = source;
    public string OwnerId { get; set; } = ownerId;
    public int EndsAt { get; set; } = endsAt;
    public double Rate { get; set; } = rate;
    public double Minimum { get; set; } = minimum;
    public string Checksum { get; set; } = checksum;
    public CertificationMilestoneStatus Status { get; set; } = status;

    /// <summary>Starts a certification milestone in status created.</summary>
    public static CertificationMilestone NewCertificationMilestone(string code, string title)
    {
        return new CertificationMilestone(
            code,
            title,
            "",
            "",
            "",
            0,
            0.0,
            0.0,
            "",
            CertificationMilestoneStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case CertificationMilestoneStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(CertificationMilestoneStatus next)
    {
        switch (Status)
        {
            case CertificationMilestoneStatus.Created:
                switch (next)
                {
                    case CertificationMilestoneStatus.Published:
                        return true;
                }
                return false;
            case CertificationMilestoneStatus.Validated:
                return next == CertificationMilestoneStatus.Published;
            case CertificationMilestoneStatus.Published:
                switch (next)
                {
                    case CertificationMilestoneStatus.Retired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(CertificationMilestoneStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"certification milestone {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        EndsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} ({Title})";
    }

    public double AdjustedLoad()
    {
        return Math.Round((double)EndsAt - Minimum, 2);
    }

    public CertificationMilestone Copy()
    {
        return new CertificationMilestone(
            Code,
            Title,
            SalaryBand,
            Source,
            OwnerId,
            EndsAt,
            Rate,
            Minimum,
            Checksum,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Source);
    }
}
