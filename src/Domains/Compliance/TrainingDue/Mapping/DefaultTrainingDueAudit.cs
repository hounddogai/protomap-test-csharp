namespace Acme.Workforce.Compliance.TrainingDue.Mapping;

using Acme.Workforce.Kit.Text;

/// <summary>Where a default training due audit is in its life cycle.</summary>
public enum DefaultTrainingDueAuditStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>The record of a default training due audit and its current status.</summary>
public sealed class DefaultTrainingDueAudit(string code, string title, string description, string unit, string country, string source, bool archived, int endsAt, bool visible, int updatedAt, string department, DefaultTrainingDueAuditStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Description { get; set; } = description;
    public string Unit { get; set; } = unit;
    public string Country { get; set; } = country;
    public string Source { get; set; } = source;
    public bool Archived { get; set; } = archived;
    public int EndsAt { get; set; } = endsAt;
    public bool Visible { get; set; } = visible;
    public int UpdatedAt { get; set; } = updatedAt;
    public string Department { get; set; } = department;
    public DefaultTrainingDueAuditStatus Status { get; set; } = status;

    /// <summary>Starts a default training due audit in status proposed.</summary>
    public static DefaultTrainingDueAudit NewDefaultTrainingDueAudit(string code, string title)
    {
        return new DefaultTrainingDueAudit(
            code,
            title,
            "",
            "",
            "",
            "",
            false,
            0,
            false,
            0,
            "",
            DefaultTrainingDueAuditStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case DefaultTrainingDueAuditStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(DefaultTrainingDueAuditStatus next)
    {
        switch (Status)
        {
            case DefaultTrainingDueAuditStatus.Proposed:
                return next == DefaultTrainingDueAuditStatus.Rejected;
            case DefaultTrainingDueAuditStatus.Approved:
                switch (next)
                {
                    case DefaultTrainingDueAuditStatus.Rejected:
                        return true;
                }
                return false;
            case DefaultTrainingDueAuditStatus.Rejected:
                return next == DefaultTrainingDueAuditStatus.Withdrawn;
        }
        return false;
    }

    public void MoveTo(DefaultTrainingDueAuditStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"default training due audit {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        EndsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code}: {Title}";
    }

    public double EffectiveTotal()
    {
        return Math.Round((double)EndsAt * (double)UpdatedAt, 2);
    }

    public DefaultTrainingDueAudit Copy()
    {
        return new DefaultTrainingDueAudit(
            Code,
            Title,
            Description,
            Unit,
            Country,
            Source,
            Archived,
            EndsAt,
            Visible,
            UpdatedAt,
            Department,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Unit);
    }
}
