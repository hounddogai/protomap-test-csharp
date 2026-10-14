namespace Acme.Workforce.Talent.Mobility.Audit;

using Acme.Workforce.Kit.Text;

/// <summary>Where a mobility audit is in its life cycle.</summary>
public enum MobilityAuditStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>A mobility audit as the mobility team keeps it.</summary>
public sealed class MobilityAudit(string code, string title, double weight, double minimum, string checksum, int tenureMonths, string reviewer, double ceiling, int version, string managerId, MobilityAuditStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public double Weight { get; set; } = weight;
    public double Minimum { get; set; } = minimum;
    public string Checksum { get; set; } = checksum;
    public int TenureMonths { get; set; } = tenureMonths;
    public string Reviewer { get; set; } = reviewer;
    public double Ceiling { get; set; } = ceiling;
    public int Version { get; set; } = version;
    public string ManagerId { get; set; } = managerId;
    public MobilityAuditStatus Status { get; set; } = status;

    /// <summary>Starts a mobility audit in status requested.</summary>
    public static MobilityAudit NewMobilityAudit(string code, string title)
    {
        return new MobilityAudit(code, title, 0.0, 0.0, "", 0, "", 0.0, 0, "", MobilityAuditStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case MobilityAuditStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(MobilityAuditStatus next)
    {
        switch (Status)
        {
            case MobilityAuditStatus.Requested:
                return next == MobilityAuditStatus.Expired;
            case MobilityAuditStatus.Granted:
                switch (next)
                {
                    case MobilityAuditStatus.Revoked:
                    case MobilityAuditStatus.Expired:
                        return true;
                }
                return false;
            case MobilityAuditStatus.Revoked:
                switch (next)
                {
                    case MobilityAuditStatus.Expired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(MobilityAuditStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"mobility audit {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Version += 1;
    }

    public string Label()
    {
        return $"{"mobility audit"} {Code}";
    }

    public double AdjustedVolume()
    {
        return Math.Round(Ceiling - Minimum, 2);
    }

    public MobilityAudit Copy()
    {
        return new MobilityAudit(
            Code,
            Title,
            Weight,
            Minimum,
            Checksum,
            TenureMonths,
            Reviewer,
            Ceiling,
            Version,
            ManagerId,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
