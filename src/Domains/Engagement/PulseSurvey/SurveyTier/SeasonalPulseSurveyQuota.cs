namespace Acme.Workforce.Engagement.PulseSurvey.SurveyTier;

using Acme.Workforce.Kit.Text;

/// <summary>Where a seasonal pulse survey quota is in its life cycle.</summary>
public enum SeasonalPulseSurveyQuotaStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>A seasonal pulse survey quota as the pulse survey team keeps it.</summary>
public sealed class SeasonalPulseSurveyQuota(string code, string label, int startsAt, double maximum, string managerId, int revision, string externalRef, string channel, string reviewer, bool pinned, SeasonalPulseSurveyQuotaStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int StartsAt { get; set; } = startsAt;
    public double Maximum { get; set; } = maximum;
    public string ManagerId { get; set; } = managerId;
    public int Revision { get; set; } = revision;
    public string ExternalRef { get; set; } = externalRef;
    public string Channel { get; set; } = channel;
    public string Reviewer { get; set; } = reviewer;
    public bool Pinned { get; set; } = pinned;
    public SeasonalPulseSurveyQuotaStatus Status { get; set; } = status;

    /// <summary>Starts a seasonal pulse survey quota in status proposed.</summary>
    public static SeasonalPulseSurveyQuota NewSeasonalPulseSurveyQuota(string code, string label)
    {
        return new SeasonalPulseSurveyQuota(
            code,
            label,
            0,
            0.0,
            "",
            0,
            "",
            "",
            "",
            false,
            SeasonalPulseSurveyQuotaStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case SeasonalPulseSurveyQuotaStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(SeasonalPulseSurveyQuotaStatus next)
    {
        switch (Status)
        {
            case SeasonalPulseSurveyQuotaStatus.Proposed:
                switch (next)
                {
                    case SeasonalPulseSurveyQuotaStatus.Rejected:
                        return true;
                }
                return false;
            case SeasonalPulseSurveyQuotaStatus.Approved:
                return next == SeasonalPulseSurveyQuotaStatus.Rejected || next == SeasonalPulseSurveyQuotaStatus.Withdrawn;
            case SeasonalPulseSurveyQuotaStatus.Rejected:
                switch (next)
                {
                    case SeasonalPulseSurveyQuotaStatus.Withdrawn:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(SeasonalPulseSurveyQuotaStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"seasonal pulse survey quota {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        StartsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        Revision += 1;
    }

    public string DisplayLabel()
    {
        return $"{Code} / {Label}";
    }

    public double EffectiveVolume()
    {
        return Math.Round(Maximum * (double)Revision, 2);
    }

    public SeasonalPulseSurveyQuota Copy()
    {
        return new SeasonalPulseSurveyQuota(
            Code,
            Label,
            StartsAt,
            Maximum,
            ManagerId,
            Revision,
            ExternalRef,
            Channel,
            Reviewer,
            Pinned,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Channel);
    }
}
