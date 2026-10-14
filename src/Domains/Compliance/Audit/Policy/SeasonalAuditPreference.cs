namespace Acme.Workforce.Compliance.Audit.Policy;

using Acme.Workforce.Kit.Text;

/// <summary>Where a seasonal audit preference is in its life cycle.</summary>
public enum SeasonalAuditPreferenceStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>A seasonal audit preference as the audit team keeps it.</summary>
public sealed class SeasonalAuditPreference(string code, string label, int batchSize, string skill, string description, string segment, double fte, int updatedAt, SeasonalAuditPreferenceStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int BatchSize { get; set; } = batchSize;
    public string Skill { get; set; } = skill;
    public string Description { get; set; } = description;
    public string Segment { get; set; } = segment;
    public double Fte { get; set; } = fte;
    public int UpdatedAt { get; set; } = updatedAt;
    public SeasonalAuditPreferenceStatus Status { get; set; } = status;

    /// <summary>Starts a seasonal audit preference in status new.</summary>
    public static SeasonalAuditPreference NewSeasonalAuditPreference(string code, string label)
    {
        return new SeasonalAuditPreference(code, label, 0, "", "", "", 0.0, 0, SeasonalAuditPreferenceStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case SeasonalAuditPreferenceStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(SeasonalAuditPreferenceStatus next)
    {
        switch (Status)
        {
            case SeasonalAuditPreferenceStatus.New:
                switch (next)
                {
                    case SeasonalAuditPreferenceStatus.Running:
                        return true;
                }
                return false;
            case SeasonalAuditPreferenceStatus.Queued:
                switch (next)
                {
                    case SeasonalAuditPreferenceStatus.Failed:
                        return true;
                }
                return false;
            case SeasonalAuditPreferenceStatus.Running:
                return next == SeasonalAuditPreferenceStatus.Failed;
            case SeasonalAuditPreferenceStatus.Done:
                return next == SeasonalAuditPreferenceStatus.Failed;
        }
        return false;
    }

    public void MoveTo(SeasonalAuditPreferenceStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"seasonal audit preference {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        UpdatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{"seasonal audit preference"} {Code}";
    }

    public double EffectiveValue()
    {
        return Math.Round((double)UpdatedAt + (double)BatchSize, 2);
    }

    public SeasonalAuditPreference Copy()
    {
        return new SeasonalAuditPreference(
            Code,
            Label,
            BatchSize,
            Skill,
            Description,
            Segment,
            Fte,
            UpdatedAt,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Segment);
    }
}
