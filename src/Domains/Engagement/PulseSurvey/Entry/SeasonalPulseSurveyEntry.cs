namespace Acme.Workforce.Engagement.PulseSurvey.Entry;

using Acme.Workforce.Kit.Text;

/// <summary>Where a seasonal pulse survey entry is in its life cycle.</summary>
public enum SeasonalPulseSurveyEntryStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>A seasonal pulse survey entry. Its code is unique within a tenant.</summary>
public sealed class SeasonalPulseSurveyEntry(string code, string label, string costCenter, bool visible, string category, int priority, int tenureMonths, string notes, string reviewer, int expiresAt, SeasonalPulseSurveyEntryStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string CostCenter { get; set; } = costCenter;
    public bool Visible { get; set; } = visible;
    public string Category { get; set; } = category;
    public int Priority { get; set; } = priority;
    public int TenureMonths { get; set; } = tenureMonths;
    public string Notes { get; set; } = notes;
    public string Reviewer { get; set; } = reviewer;
    public int ExpiresAt { get; set; } = expiresAt;
    public SeasonalPulseSurveyEntryStatus Status { get; set; } = status;

    /// <summary>Starts a seasonal pulse survey entry in status idle.</summary>
    public static SeasonalPulseSurveyEntry NewSeasonalPulseSurveyEntry(string code, string label)
    {
        return new SeasonalPulseSurveyEntry(
            code,
            label,
            "",
            false,
            "",
            0,
            0,
            "",
            "",
            0,
            SeasonalPulseSurveyEntryStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case SeasonalPulseSurveyEntryStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(SeasonalPulseSurveyEntryStatus next)
    {
        switch (Status)
        {
            case SeasonalPulseSurveyEntryStatus.Idle:
                switch (next)
                {
                    case SeasonalPulseSurveyEntryStatus.Warming:
                    case SeasonalPulseSurveyEntryStatus.Stopped:
                        return true;
                }
                return false;
            case SeasonalPulseSurveyEntryStatus.Warming:
                switch (next)
                {
                    case SeasonalPulseSurveyEntryStatus.Draining:
                    case SeasonalPulseSurveyEntryStatus.Stopped:
                        return true;
                }
                return false;
            case SeasonalPulseSurveyEntryStatus.Ready:
                return next == SeasonalPulseSurveyEntryStatus.Stopped;
            case SeasonalPulseSurveyEntryStatus.Draining:
                switch (next)
                {
                    case SeasonalPulseSurveyEntryStatus.Stopped:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(SeasonalPulseSurveyEntryStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"seasonal pulse survey entry {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        ExpiresAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{Code} ({Label})";
    }

    public double AdjustedVolume()
    {
        return Math.Round((double)TenureMonths * (double)Priority, 2);
    }

    public SeasonalPulseSurveyEntry Copy()
    {
        return new SeasonalPulseSurveyEntry(
            Code,
            Label,
            CostCenter,
            Visible,
            Category,
            Priority,
            TenureMonths,
            Notes,
            Reviewer,
            ExpiresAt,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Category);
    }
}
