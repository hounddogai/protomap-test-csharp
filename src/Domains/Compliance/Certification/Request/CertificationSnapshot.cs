namespace Acme.Workforce.Compliance.Certification.Request;

using Acme.Workforce.Kit.Text;

/// <summary>Where a certification snapshot is in its life cycle.</summary>
public enum CertificationSnapshotStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>A certification snapshot. Its code is unique within a tenant.</summary>
public sealed class CertificationSnapshot(string code, string label, double hours, string source, double amount, double discount, CertificationSnapshotStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public double Hours { get; set; } = hours;
    public string Source { get; set; } = source;
    public double Amount { get; set; } = amount;
    public double Discount { get; set; } = discount;
    public CertificationSnapshotStatus Status { get; set; } = status;

    /// <summary>Starts a certification snapshot in status requested.</summary>
    public static CertificationSnapshot NewCertificationSnapshot(string code, string label)
    {
        return new CertificationSnapshot(code, label, 0.0, "", 0.0, 0.0, CertificationSnapshotStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case CertificationSnapshotStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(CertificationSnapshotStatus next)
    {
        switch (Status)
        {
            case CertificationSnapshotStatus.Requested:
                switch (next)
                {
                    case CertificationSnapshotStatus.Granted:
                        return true;
                }
                return false;
            case CertificationSnapshotStatus.Granted:
                return next == CertificationSnapshotStatus.Revoked || next == CertificationSnapshotStatus.Expired;
            case CertificationSnapshotStatus.Revoked:
                switch (next)
                {
                    case CertificationSnapshotStatus.Expired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(CertificationSnapshotStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"certification snapshot {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{"certification snapshot"} {Code}";
    }

    public double NetVolume()
    {
        return Math.Round(Discount + Hours, 2);
    }

    public CertificationSnapshot Copy()
    {
        return new CertificationSnapshot(Code, Label, Hours, Source, Amount, Discount, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Source);
    }
}
