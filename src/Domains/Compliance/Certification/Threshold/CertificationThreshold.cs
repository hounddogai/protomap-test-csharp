namespace Acme.Workforce.Compliance.Certification.Threshold;

using Acme.Workforce.Kit.Text;

/// <summary>Where a certification threshold is in its life cycle.</summary>
public enum CertificationThresholdStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>The record of a certification threshold and its current status.</summary>
public sealed class CertificationThreshold(string code, string title, string phone, bool approved, string costCenter, string source, double fte, int retries, int limit, string segment, CertificationThresholdStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Phone { get; set; } = phone;
    public bool Approved { get; set; } = approved;
    public string CostCenter { get; set; } = costCenter;
    public string Source { get; set; } = source;
    public double Fte { get; set; } = fte;
    public int Retries { get; set; } = retries;
    public int Limit { get; set; } = limit;
    public string Segment { get; set; } = segment;
    public CertificationThresholdStatus Status { get; set; } = status;

    /// <summary>Starts a certification threshold in status new.</summary>
    public static CertificationThreshold NewCertificationThreshold(string code, string title)
    {
        return new CertificationThreshold(
            code,
            title,
            "",
            false,
            "",
            "",
            0.0,
            0,
            0,
            "",
            CertificationThresholdStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case CertificationThresholdStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(CertificationThresholdStatus next)
    {
        switch (Status)
        {
            case CertificationThresholdStatus.New:
                return next == CertificationThresholdStatus.Running;
            case CertificationThresholdStatus.Queued:
                return next == CertificationThresholdStatus.Running || next == CertificationThresholdStatus.Failed;
            case CertificationThresholdStatus.Running:
                return next == CertificationThresholdStatus.Done;
            case CertificationThresholdStatus.Done:
                switch (next)
                {
                    case CertificationThresholdStatus.Failed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(CertificationThresholdStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"certification threshold {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} [{Title}]";
    }

    public double WeightedExposure()
    {
        return Math.Round(Fte + (double)Retries, 2);
    }

    public CertificationThreshold Copy()
    {
        return new CertificationThreshold(
            Code,
            Title,
            Phone,
            Approved,
            CostCenter,
            Source,
            Fte,
            Retries,
            Limit,
            Segment,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Source);
    }
}
