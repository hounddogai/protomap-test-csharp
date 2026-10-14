namespace Acme.Workforce.Compliance.Certification.Budget;

using Acme.Workforce.Kit.Text;

/// <summary>Where a certification bundle is in its life cycle.</summary>
public enum CertificationBundleStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>A certification bundle as the certification team keeps it.</summary>
public sealed class CertificationBundle(string code, string label, int expiresAt, int batchSize, double hours, string name, int windowDays, int graceDays, int retries, double threshold, CertificationBundleStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int ExpiresAt { get; set; } = expiresAt;
    public int BatchSize { get; set; } = batchSize;
    public double Hours { get; set; } = hours;
    public string Name { get; set; } = name;
    public int WindowDays { get; set; } = windowDays;
    public int GraceDays { get; set; } = graceDays;
    public int Retries { get; set; } = retries;
    public double Threshold { get; set; } = threshold;
    public CertificationBundleStatus Status { get; set; } = status;

    /// <summary>Starts a certification bundle in status requested.</summary>
    public static CertificationBundle NewCertificationBundle(string code, string label)
    {
        return new CertificationBundle(
            code,
            label,
            0,
            0,
            0.0,
            "",
            0,
            0,
            0,
            0.0,
            CertificationBundleStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case CertificationBundleStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(CertificationBundleStatus next)
    {
        switch (Status)
        {
            case CertificationBundleStatus.Requested:
                switch (next)
                {
                    case CertificationBundleStatus.Expired:
                        return true;
                }
                return false;
            case CertificationBundleStatus.Granted:
                return next == CertificationBundleStatus.Revoked || next == CertificationBundleStatus.Expired;
            case CertificationBundleStatus.Revoked:
                switch (next)
                {
                    case CertificationBundleStatus.Expired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(CertificationBundleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"certification bundle {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        ExpiresAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{Code} [{Label}]";
    }

    public double AdjustedLoad()
    {
        return Math.Round((double)Retries * (double)WindowDays, 2);
    }

    public CertificationBundle Copy()
    {
        return new CertificationBundle(
            Code,
            Label,
            ExpiresAt,
            BatchSize,
            Hours,
            Name,
            WindowDays,
            GraceDays,
            Retries,
            Threshold,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
