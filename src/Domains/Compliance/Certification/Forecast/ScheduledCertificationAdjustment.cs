namespace Acme.Workforce.Compliance.Certification.Forecast;

using Acme.Workforce.Kit.Text;

/// <summary>Where a scheduled certification adjustment is in its life cycle.</summary>
public enum ScheduledCertificationAdjustmentStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>One scheduled certification adjustment, with what the planners need to act on it.</summary>
public sealed class ScheduledCertificationAdjustment(string code, string name, string salaryBand, string contactEmail, string channel, double weight, int windowDays, string managerId, int expiresAt, ScheduledCertificationAdjustmentStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string SalaryBand { get; set; } = salaryBand;
    public string ContactEmail { get; set; } = contactEmail;
    public string Channel { get; set; } = channel;
    public double Weight { get; set; } = weight;
    public int WindowDays { get; set; } = windowDays;
    public string ManagerId { get; set; } = managerId;
    public int ExpiresAt { get; set; } = expiresAt;
    public ScheduledCertificationAdjustmentStatus Status { get; set; } = status;

    /// <summary>Starts a scheduled certification adjustment in status planned.</summary>
    public static ScheduledCertificationAdjustment NewScheduledCertificationAdjustment(string code, string name)
    {
        return new ScheduledCertificationAdjustment(
            code,
            name,
            "",
            "",
            "",
            0.0,
            0,
            "",
            0,
            ScheduledCertificationAdjustmentStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ScheduledCertificationAdjustmentStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ScheduledCertificationAdjustmentStatus next)
    {
        switch (Status)
        {
            case ScheduledCertificationAdjustmentStatus.Planned:
                switch (next)
                {
                    case ScheduledCertificationAdjustmentStatus.Scheduled:
                    case ScheduledCertificationAdjustmentStatus.Cancelled:
                        return true;
                }
                return false;
            case ScheduledCertificationAdjustmentStatus.Scheduled:
                switch (next)
                {
                    case ScheduledCertificationAdjustmentStatus.Completed:
                        return true;
                }
                return false;
            case ScheduledCertificationAdjustmentStatus.InProgress:
                switch (next)
                {
                    case ScheduledCertificationAdjustmentStatus.Completed:
                    case ScheduledCertificationAdjustmentStatus.Cancelled:
                        return true;
                }
                return false;
            case ScheduledCertificationAdjustmentStatus.Completed:
                return next == ScheduledCertificationAdjustmentStatus.Cancelled;
        }
        return false;
    }

    public void MoveTo(ScheduledCertificationAdjustmentStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"scheduled certification adjustment {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        ExpiresAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} ({Name})";
    }

    public double WeightedExposure()
    {
        return Math.Round((double)ExpiresAt + Weight, 2);
    }

    public ScheduledCertificationAdjustment Copy()
    {
        return new ScheduledCertificationAdjustment(
            Code,
            Name,
            SalaryBand,
            ContactEmail,
            Channel,
            Weight,
            WindowDays,
            ManagerId,
            ExpiresAt,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Channel);
    }
}
