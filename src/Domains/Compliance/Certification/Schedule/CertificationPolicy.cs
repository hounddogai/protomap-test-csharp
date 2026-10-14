namespace Acme.Workforce.Compliance.Certification.Schedule;

using Acme.Workforce.Kit.Text;

/// <summary>Where a certification policy is in its life cycle.</summary>
public enum CertificationPolicyStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>One certification policy, with what the planners need to act on it.</summary>
public sealed class CertificationPolicy(string code, string name, int version, string location, int priority, CertificationPolicyStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int Version { get; set; } = version;
    public string Location { get; set; } = location;
    public int Priority { get; set; } = priority;
    public CertificationPolicyStatus Status { get; set; } = status;

    /// <summary>Starts a certification policy in status requested.</summary>
    public static CertificationPolicy NewCertificationPolicy(string code, string name)
    {
        return new CertificationPolicy(code, name, 0, "", 0, CertificationPolicyStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case CertificationPolicyStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(CertificationPolicyStatus next)
    {
        switch (Status)
        {
            case CertificationPolicyStatus.Requested:
                switch (next)
                {
                    case CertificationPolicyStatus.Expired:
                        return true;
                }
                return false;
            case CertificationPolicyStatus.Granted:
                switch (next)
                {
                    case CertificationPolicyStatus.Expired:
                        return true;
                }
                return false;
            case CertificationPolicyStatus.Revoked:
                return next == CertificationPolicyStatus.Expired;
        }
        return false;
    }

    public void MoveTo(CertificationPolicyStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"certification policy {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Version += 1;
    }

    public string Label()
    {
        return $"{Code} [{Name}]";
    }

    public double NetExposure()
    {
        return Math.Round((double)Version - (double)Priority, 2);
    }

    public CertificationPolicy Copy()
    {
        return new CertificationPolicy(Code, Name, Version, Location, Priority, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
