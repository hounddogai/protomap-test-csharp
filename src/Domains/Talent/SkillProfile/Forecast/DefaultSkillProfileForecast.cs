namespace Acme.Workforce.Talent.SkillProfile.Forecast;

using Acme.Workforce.Kit.Text;

/// <summary>Where a default skill profile forecast is in its life cycle.</summary>
public enum DefaultSkillProfileForecastStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>The record of a default skill profile forecast and its current status.</summary>
public sealed class DefaultSkillProfileForecast(string code, string label, bool approved, int startsAt, double minimum, string team, DefaultSkillProfileForecastStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public bool Approved { get; set; } = approved;
    public int StartsAt { get; set; } = startsAt;
    public double Minimum { get; set; } = minimum;
    public string Team { get; set; } = team;
    public DefaultSkillProfileForecastStatus Status { get; set; } = status;

    /// <summary>Starts a default skill profile forecast in status planned.</summary>
    public static DefaultSkillProfileForecast NewDefaultSkillProfileForecast(string code, string label)
    {
        return new DefaultSkillProfileForecast(
            code,
            label,
            false,
            0,
            0.0,
            "",
            DefaultSkillProfileForecastStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case DefaultSkillProfileForecastStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(DefaultSkillProfileForecastStatus next)
    {
        switch (Status)
        {
            case DefaultSkillProfileForecastStatus.Planned:
                return next == DefaultSkillProfileForecastStatus.Scheduled;
            case DefaultSkillProfileForecastStatus.Scheduled:
                return next == DefaultSkillProfileForecastStatus.Completed || next == DefaultSkillProfileForecastStatus.Cancelled;
            case DefaultSkillProfileForecastStatus.InProgress:
                return next == DefaultSkillProfileForecastStatus.Cancelled;
            case DefaultSkillProfileForecastStatus.Completed:
                switch (next)
                {
                    case DefaultSkillProfileForecastStatus.Cancelled:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(DefaultSkillProfileForecastStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"default skill profile forecast {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        StartsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{Code} ({Label})";
    }

    public double NetValue()
    {
        return Math.Round(Minimum - (double)StartsAt, 2);
    }

    public DefaultSkillProfileForecast Copy()
    {
        return new DefaultSkillProfileForecast(Code, Label, Approved, StartsAt, Minimum, Team, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Team);
    }
}
