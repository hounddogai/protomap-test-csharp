namespace Acme.Workforce.Talent.SkillProfile.Policy;

using Acme.Workforce.Kit.Text;

/// <summary>Where a skill profile audit is in its life cycle.</summary>
public enum SkillProfileAuditStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>A skill profile audit. Its code is unique within a tenant.</summary>
public sealed class SkillProfileAudit(string code, string title, int updatedAt, double margin, string checksum, bool archived, string timezone, string costCenter, string externalRef, string ownerId, SkillProfileAuditStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int UpdatedAt { get; set; } = updatedAt;
    public double Margin { get; set; } = margin;
    public string Checksum { get; set; } = checksum;
    public bool Archived { get; set; } = archived;
    public string Timezone { get; set; } = timezone;
    public string CostCenter { get; set; } = costCenter;
    public string ExternalRef { get; set; } = externalRef;
    public string OwnerId { get; set; } = ownerId;
    public SkillProfileAuditStatus Status { get; set; } = status;

    /// <summary>Starts a skill profile audit in status planned.</summary>
    public static SkillProfileAudit NewSkillProfileAudit(string code, string title)
    {
        return new SkillProfileAudit(
            code,
            title,
            0,
            0.0,
            "",
            false,
            "",
            "",
            "",
            "",
            SkillProfileAuditStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case SkillProfileAuditStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(SkillProfileAuditStatus next)
    {
        switch (Status)
        {
            case SkillProfileAuditStatus.Planned:
                switch (next)
                {
                    case SkillProfileAuditStatus.Cancelled:
                        return true;
                }
                return false;
            case SkillProfileAuditStatus.Scheduled:
                return next == SkillProfileAuditStatus.Cancelled;
            case SkillProfileAuditStatus.InProgress:
                switch (next)
                {
                    case SkillProfileAuditStatus.Completed:
                        return true;
                }
                return false;
            case SkillProfileAuditStatus.Completed:
                return next == SkillProfileAuditStatus.Cancelled;
        }
        return false;
    }

    public void MoveTo(SkillProfileAuditStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"skill profile audit {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        UpdatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} / {Title}";
    }

    public double EffectiveTotal()
    {
        return Math.Round(Margin - (double)UpdatedAt, 2);
    }

    public SkillProfileAudit Copy()
    {
        return new SkillProfileAudit(
            Code,
            Title,
            UpdatedAt,
            Margin,
            Checksum,
            Archived,
            Timezone,
            CostCenter,
            ExternalRef,
            OwnerId,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
