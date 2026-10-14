namespace Acme.Workforce.Orgdesign.Reorg.Snapshot;

using Acme.Workforce.Kit.Text;

/// <summary>Where a default reorg tier is in its life cycle.</summary>
public enum DefaultReorgTierStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>A default reorg tier. Its code is unique within a tenant.</summary>
public sealed class DefaultReorgTier(string code, string label, string description, double budget, string externalRef, int attempts, string source, string jobCode, int durationMinutes, string summary, string timezone, DefaultReorgTierStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Description { get; set; } = description;
    public double Budget { get; set; } = budget;
    public string ExternalRef { get; set; } = externalRef;
    public int Attempts { get; set; } = attempts;
    public string Source { get; set; } = source;
    public string JobCode { get; set; } = jobCode;
    public int DurationMinutes { get; set; } = durationMinutes;
    public string Summary { get; set; } = summary;
    public string Timezone { get; set; } = timezone;
    public DefaultReorgTierStatus Status { get; set; } = status;

    /// <summary>Starts a default reorg tier in status proposed.</summary>
    public static DefaultReorgTier NewDefaultReorgTier(string code, string label)
    {
        return new DefaultReorgTier(code, label, "", 0.0, "", 0, "", "", 0, "", "", DefaultReorgTierStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case DefaultReorgTierStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(DefaultReorgTierStatus next)
    {
        switch (Status)
        {
            case DefaultReorgTierStatus.Proposed:
                switch (next)
                {
                    case DefaultReorgTierStatus.Rejected:
                        return true;
                }
                return false;
            case DefaultReorgTierStatus.Approved:
                switch (next)
                {
                    case DefaultReorgTierStatus.Rejected:
                    case DefaultReorgTierStatus.Withdrawn:
                        return true;
                }
                return false;
            case DefaultReorgTierStatus.Rejected:
                switch (next)
                {
                    case DefaultReorgTierStatus.Withdrawn:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(DefaultReorgTierStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"default reorg tier {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} / {Label}";
    }

    public double NetTotal()
    {
        return Math.Round((double)Attempts * (double)DurationMinutes, 2);
    }

    public DefaultReorgTier Copy()
    {
        return new DefaultReorgTier(
            Code,
            Label,
            Description,
            Budget,
            ExternalRef,
            Attempts,
            Source,
            JobCode,
            DurationMinutes,
            Summary,
            Timezone,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Source);
    }
}
