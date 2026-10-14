namespace Acme.Workforce.Talent.SkillProfile.Notice;

using Acme.Workforce.Kit.Text;

/// <summary>Where a pending skill profile forecast is in its life cycle.</summary>
public enum PendingSkillProfileForecastStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>A pending skill profile forecast as the skill profile team keeps it.</summary>
public sealed class PendingSkillProfileForecast(string code, string title, Dictionary<string, string> attributes, string summary, int attempts, PendingSkillProfileForecastStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public string Summary { get; set; } = summary;
    public int Attempts { get; set; } = attempts;
    public PendingSkillProfileForecastStatus Status { get; set; } = status;

    /// <summary>Starts a pending skill profile forecast in status planned.</summary>
    public static PendingSkillProfileForecast NewPendingSkillProfileForecast(string code, string title)
    {
        return new PendingSkillProfileForecast(
            code,
            title,
            new Dictionary<string, string>(),
            "",
            0,
            PendingSkillProfileForecastStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PendingSkillProfileForecastStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PendingSkillProfileForecastStatus next)
    {
        switch (Status)
        {
            case PendingSkillProfileForecastStatus.Planned:
                return next == PendingSkillProfileForecastStatus.Scheduled;
            case PendingSkillProfileForecastStatus.Scheduled:
                return next == PendingSkillProfileForecastStatus.Completed;
            case PendingSkillProfileForecastStatus.InProgress:
                switch (next)
                {
                    case PendingSkillProfileForecastStatus.Completed:
                    case PendingSkillProfileForecastStatus.Cancelled:
                        return true;
                }
                return false;
            case PendingSkillProfileForecastStatus.Completed:
                switch (next)
                {
                    case PendingSkillProfileForecastStatus.Cancelled:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(PendingSkillProfileForecastStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"pending skill profile forecast {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code}: {Title}";
    }

    public string Attribute(string key, string fallback)
    {
        if (!Attributes.TryGetValue(key, out var found))
        {
            return fallback;
        }
        if (found.Trim() == "")
        {
            return fallback;
        }
        return found;
    }

    public double MeasuredValue()
    {
        return (double)Attempts;
    }

    public PendingSkillProfileForecast Copy()
    {
        return new PendingSkillProfileForecast(Code, Title, Attributes, Summary, Attempts, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
