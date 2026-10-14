namespace Acme.Workforce.Compliance.Certification.Window;

using Acme.Workforce.Kit.Text;

/// <summary>Where a certification group is in its life cycle.</summary>
public enum CertificationGroupStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>A certification group as the certification team keeps it.</summary>
public sealed class CertificationGroup(string code, string name, double amount, string summary, string managerId, string timezone, int endsAt, int retries, CertificationGroupStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public double Amount { get; set; } = amount;
    public string Summary { get; set; } = summary;
    public string ManagerId { get; set; } = managerId;
    public string Timezone { get; set; } = timezone;
    public int EndsAt { get; set; } = endsAt;
    public int Retries { get; set; } = retries;
    public CertificationGroupStatus Status { get; set; } = status;

    /// <summary>Starts a certification group in status planned.</summary>
    public static CertificationGroup NewCertificationGroup(string code, string name)
    {
        return new CertificationGroup(code, name, 0.0, "", "", "", 0, 0, CertificationGroupStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case CertificationGroupStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(CertificationGroupStatus next)
    {
        switch (Status)
        {
            case CertificationGroupStatus.Planned:
                switch (next)
                {
                    case CertificationGroupStatus.Completed:
                        return true;
                }
                return false;
            case CertificationGroupStatus.Scheduled:
                return next == CertificationGroupStatus.InProgress;
            case CertificationGroupStatus.InProgress:
                return next == CertificationGroupStatus.Completed || next == CertificationGroupStatus.Cancelled;
            case CertificationGroupStatus.Completed:
                return next == CertificationGroupStatus.Cancelled;
        }
        return false;
    }

    public void MoveTo(CertificationGroupStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"certification group {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        EndsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} / {Name}";
    }

    public double NetLoad()
    {
        return Math.Round((double)Retries * (double)EndsAt, 2);
    }

    public CertificationGroup Copy()
    {
        return new CertificationGroup(Code, Name, Amount, Summary, ManagerId, Timezone, EndsAt, Retries, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
