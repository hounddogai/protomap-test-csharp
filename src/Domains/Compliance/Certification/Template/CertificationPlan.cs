namespace Acme.Workforce.Compliance.Certification.Template;

using Acme.Workforce.Kit.Text;

/// <summary>Where a certification plan is in its life cycle.</summary>
public enum CertificationPlanStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>A certification plan. Its code is unique within a tenant.</summary>
public sealed class CertificationPlan(string code, string name, int version, double floorValue, double score, string grade, string badge, string channel, int durationMinutes, CertificationPlanStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int Version { get; set; } = version;
    public double FloorValue { get; set; } = floorValue;
    public double Score { get; set; } = score;
    public string Grade { get; set; } = grade;
    public string Badge { get; set; } = badge;
    public string Channel { get; set; } = channel;
    public int DurationMinutes { get; set; } = durationMinutes;
    public CertificationPlanStatus Status { get; set; } = status;

    /// <summary>Starts a certification plan in status planned.</summary>
    public static CertificationPlan NewCertificationPlan(string code, string name)
    {
        return new CertificationPlan(code, name, 0, 0.0, 0.0, "", "", "", 0, CertificationPlanStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case CertificationPlanStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(CertificationPlanStatus next)
    {
        switch (Status)
        {
            case CertificationPlanStatus.Planned:
                switch (next)
                {
                    case CertificationPlanStatus.Scheduled:
                    case CertificationPlanStatus.Cancelled:
                        return true;
                }
                return false;
            case CertificationPlanStatus.Scheduled:
                switch (next)
                {
                    case CertificationPlanStatus.Completed:
                        return true;
                }
                return false;
            case CertificationPlanStatus.InProgress:
                return next == CertificationPlanStatus.Completed;
            case CertificationPlanStatus.Completed:
                switch (next)
                {
                    case CertificationPlanStatus.Cancelled:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(CertificationPlanStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"certification plan {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Version += 1;
    }

    public string Label()
    {
        return $"{Code}: {Name}";
    }

    public double AdjustedValue()
    {
        return Math.Round(FloorValue * (double)Version, 2);
    }

    public CertificationPlan Copy()
    {
        return new CertificationPlan(
            Code,
            Name,
            Version,
            FloorValue,
            Score,
            Grade,
            Badge,
            Channel,
            DurationMinutes,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Channel);
    }
}
