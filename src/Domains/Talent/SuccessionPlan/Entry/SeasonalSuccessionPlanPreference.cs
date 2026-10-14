namespace Acme.Workforce.Talent.SuccessionPlan.Entry;

using Acme.Workforce.Kit.Text;

/// <summary>Where a seasonal succession plan preference is in its life cycle.</summary>
public enum SeasonalSuccessionPlanPreferenceStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>A seasonal succession plan preference. Its code is unique within a tenant.</summary>
public sealed class SeasonalSuccessionPlanPreference(string code, string title, double score, string locale, string phone, int startsAt, double cost, int windowDays, int attempts, string summary, bool active, SeasonalSuccessionPlanPreferenceStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public double Score { get; set; } = score;
    public string Locale { get; set; } = locale;
    public string Phone { get; set; } = phone;
    public int StartsAt { get; set; } = startsAt;
    public double Cost { get; set; } = cost;
    public int WindowDays { get; set; } = windowDays;
    public int Attempts { get; set; } = attempts;
    public string Summary { get; set; } = summary;
    public bool Active { get; set; } = active;
    public SeasonalSuccessionPlanPreferenceStatus Status { get; set; } = status;

    /// <summary>Starts a seasonal succession plan preference in status draft.</summary>
    public static SeasonalSuccessionPlanPreference NewSeasonalSuccessionPlanPreference(string code, string title)
    {
        return new SeasonalSuccessionPlanPreference(
            code,
            title,
            0.0,
            "",
            "",
            0,
            0.0,
            0,
            0,
            "",
            false,
            SeasonalSuccessionPlanPreferenceStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case SeasonalSuccessionPlanPreferenceStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(SeasonalSuccessionPlanPreferenceStatus next)
    {
        switch (Status)
        {
            case SeasonalSuccessionPlanPreferenceStatus.Draft:
                switch (next)
                {
                    case SeasonalSuccessionPlanPreferenceStatus.Suspended:
                        return true;
                }
                return false;
            case SeasonalSuccessionPlanPreferenceStatus.Pending:
                return next == SeasonalSuccessionPlanPreferenceStatus.Closed;
            case SeasonalSuccessionPlanPreferenceStatus.Active:
                switch (next)
                {
                    case SeasonalSuccessionPlanPreferenceStatus.Suspended:
                        return true;
                }
                return false;
            case SeasonalSuccessionPlanPreferenceStatus.Suspended:
                switch (next)
                {
                    case SeasonalSuccessionPlanPreferenceStatus.Closed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(SeasonalSuccessionPlanPreferenceStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"seasonal succession plan preference {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        StartsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} ({Title})";
    }

    public double ProjectedVolume()
    {
        return Math.Round((double)WindowDays - (double)StartsAt, 2);
    }

    public SeasonalSuccessionPlanPreference Copy()
    {
        return new SeasonalSuccessionPlanPreference(
            Code,
            Title,
            Score,
            Locale,
            Phone,
            StartsAt,
            Cost,
            WindowDays,
            Attempts,
            Summary,
            Active,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Locale);
    }
}
