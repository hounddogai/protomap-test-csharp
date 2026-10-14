namespace Acme.Workforce.Compliance.Certification.Schedule;

using Acme.Workforce.Kit.Text;

/// <summary>Where a certification schedule is in its life cycle.</summary>
public enum CertificationScheduleStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>A certification schedule. Its code is unique within a tenant.</summary>
public sealed class CertificationSchedule(string code, string title, int updatedAt, bool visible, string team, double budget, int retries, int quantity, string summary, CertificationScheduleStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int UpdatedAt { get; set; } = updatedAt;
    public bool Visible { get; set; } = visible;
    public string Team { get; set; } = team;
    public double Budget { get; set; } = budget;
    public int Retries { get; set; } = retries;
    public int Quantity { get; set; } = quantity;
    public string Summary { get; set; } = summary;
    public CertificationScheduleStatus Status { get; set; } = status;

    /// <summary>Starts a certification schedule in status planned.</summary>
    public static CertificationSchedule NewCertificationSchedule(string code, string title)
    {
        return new CertificationSchedule(
            code,
            title,
            0,
            false,
            "",
            0.0,
            0,
            0,
            "",
            CertificationScheduleStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case CertificationScheduleStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(CertificationScheduleStatus next)
    {
        switch (Status)
        {
            case CertificationScheduleStatus.Planned:
                switch (next)
                {
                    case CertificationScheduleStatus.InProgress:
                    case CertificationScheduleStatus.Completed:
                        return true;
                }
                return false;
            case CertificationScheduleStatus.Scheduled:
                return next == CertificationScheduleStatus.InProgress;
            case CertificationScheduleStatus.InProgress:
                return next == CertificationScheduleStatus.Completed || next == CertificationScheduleStatus.Cancelled;
            case CertificationScheduleStatus.Completed:
                return next == CertificationScheduleStatus.Cancelled;
        }
        return false;
    }

    public void MoveTo(CertificationScheduleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"certification schedule {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        UpdatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} ({Title})";
    }

    public double NetTotal()
    {
        return Math.Round((double)UpdatedAt - (double)Retries, 2);
    }

    public CertificationSchedule Copy()
    {
        return new CertificationSchedule(
            Code,
            Title,
            UpdatedAt,
            Visible,
            Team,
            Budget,
            Retries,
            Quantity,
            Summary,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Team);
    }
}
