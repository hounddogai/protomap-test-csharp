namespace Acme.Workforce.Compliance.Certification.CertificationForecast;

using Acme.Workforce.Kit.Text;

/// <summary>Where a capped certification template is in its life cycle.</summary>
public enum CappedCertificationTemplateStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>The record of a capped certification template and its current status.</summary>
public sealed class CappedCertificationTemplate(string code, string label, int revision, int attempts, int graceDays, int priority, string title, string department, CappedCertificationTemplateStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int Revision { get; set; } = revision;
    public int Attempts { get; set; } = attempts;
    public int GraceDays { get; set; } = graceDays;
    public int Priority { get; set; } = priority;
    public string Title { get; set; } = title;
    public string Department { get; set; } = department;
    public CappedCertificationTemplateStatus Status { get; set; } = status;

    /// <summary>Starts a capped certification template in status planned.</summary>
    public static CappedCertificationTemplate NewCappedCertificationTemplate(string code, string label)
    {
        return new CappedCertificationTemplate(
            code,
            label,
            0,
            0,
            0,
            0,
            "",
            "",
            CappedCertificationTemplateStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case CappedCertificationTemplateStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(CappedCertificationTemplateStatus next)
    {
        switch (Status)
        {
            case CappedCertificationTemplateStatus.Planned:
                return next == CappedCertificationTemplateStatus.InProgress || next == CappedCertificationTemplateStatus.Completed;
            case CappedCertificationTemplateStatus.Scheduled:
                switch (next)
                {
                    case CappedCertificationTemplateStatus.InProgress:
                        return true;
                }
                return false;
            case CappedCertificationTemplateStatus.InProgress:
                return next == CappedCertificationTemplateStatus.Completed;
            case CappedCertificationTemplateStatus.Completed:
                return next == CappedCertificationTemplateStatus.Cancelled;
        }
        return false;
    }

    public void MoveTo(CappedCertificationTemplateStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"capped certification template {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Revision += 1;
    }

    public string DisplayLabel()
    {
        return $"{Code} ({Label})";
    }

    public double NetTotal()
    {
        return Math.Round((double)Attempts * (double)GraceDays, 2);
    }

    public CappedCertificationTemplate Copy()
    {
        return new CappedCertificationTemplate(
            Code,
            Label,
            Revision,
            Attempts,
            GraceDays,
            Priority,
            Title,
            Department,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Department);
    }
}
