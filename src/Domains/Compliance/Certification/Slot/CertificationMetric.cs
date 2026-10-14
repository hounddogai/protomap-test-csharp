namespace Acme.Workforce.Compliance.Certification.Slot;

using Acme.Workforce.Kit.Text;

/// <summary>Where a certification metric is in its life cycle.</summary>
public enum CertificationMetricStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>A certification metric as the certification team keeps it.</summary>
public sealed class CertificationMetric(string code, string label, int graceDays, string ownerId, string timezone, int tenureMonths, string checksum, double cost, CertificationMetricStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int GraceDays { get; set; } = graceDays;
    public string OwnerId { get; set; } = ownerId;
    public string Timezone { get; set; } = timezone;
    public int TenureMonths { get; set; } = tenureMonths;
    public string Checksum { get; set; } = checksum;
    public double Cost { get; set; } = cost;
    public CertificationMetricStatus Status { get; set; } = status;

    /// <summary>Starts a certification metric in status proposed.</summary>
    public static CertificationMetric NewCertificationMetric(string code, string label)
    {
        return new CertificationMetric(code, label, 0, "", "", 0, "", 0.0, CertificationMetricStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case CertificationMetricStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(CertificationMetricStatus next)
    {
        switch (Status)
        {
            case CertificationMetricStatus.Proposed:
                return next == CertificationMetricStatus.Rejected;
            case CertificationMetricStatus.Approved:
                switch (next)
                {
                    case CertificationMetricStatus.Rejected:
                        return true;
                }
                return false;
            case CertificationMetricStatus.Rejected:
                return next == CertificationMetricStatus.Withdrawn;
        }
        return false;
    }

    public void MoveTo(CertificationMetricStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"certification metric {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} [{Label}]";
    }

    public double AdjustedTotal()
    {
        return Math.Round((double)TenureMonths * (double)GraceDays, 2);
    }

    public CertificationMetric Copy()
    {
        return new CertificationMetric(
            Code,
            Label,
            GraceDays,
            OwnerId,
            Timezone,
            TenureMonths,
            Checksum,
            Cost,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
