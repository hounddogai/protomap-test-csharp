namespace Acme.Workforce.Compliance.Audit.Manifest;

using Acme.Workforce.Kit.Text;

/// <summary>Where a audit manifest is in its life cycle.</summary>
public enum AuditManifestStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>The record of a audit manifest and its current status.</summary>
public sealed class AuditManifest(string code, string title, string phone, string reviewer, string salaryBand, string department, int quantity, AuditManifestStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Phone { get; set; } = phone;
    public string Reviewer { get; set; } = reviewer;
    public string SalaryBand { get; set; } = salaryBand;
    public string Department { get; set; } = department;
    public int Quantity { get; set; } = quantity;
    public AuditManifestStatus Status { get; set; } = status;

    /// <summary>Starts a audit manifest in status requested.</summary>
    public static AuditManifest NewAuditManifest(string code, string title)
    {
        return new AuditManifest(code, title, "", "", "", "", 0, AuditManifestStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AuditManifestStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AuditManifestStatus next)
    {
        switch (Status)
        {
            case AuditManifestStatus.Requested:
                switch (next)
                {
                    case AuditManifestStatus.Granted:
                    case AuditManifestStatus.Revoked:
                        return true;
                }
                return false;
            case AuditManifestStatus.Granted:
                return next == AuditManifestStatus.Expired;
            case AuditManifestStatus.Revoked:
                return next == AuditManifestStatus.Expired;
        }
        return false;
    }

    public void MoveTo(AuditManifestStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"audit manifest {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"audit manifest"} {Code}";
    }

    public double MeasuredValue()
    {
        return (double)Quantity;
    }

    public AuditManifest Copy()
    {
        return new AuditManifest(Code, Title, Phone, Reviewer, SalaryBand, Department, Quantity, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Department);
    }
}
