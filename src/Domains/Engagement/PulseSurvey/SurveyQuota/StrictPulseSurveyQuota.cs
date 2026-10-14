namespace Acme.Workforce.Engagement.PulseSurvey.SurveyQuota;

using Acme.Workforce.Kit.Text;

/// <summary>Where a strict pulse survey quota is in its life cycle.</summary>
public enum StrictPulseSurveyQuotaStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>A strict pulse survey quota as the pulse survey team keeps it.</summary>
public sealed class StrictPulseSurveyQuota(string code, string name, string contactEmail, int capacity, string department, int windowDays, double weight, string title, string description, StrictPulseSurveyQuotaStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string ContactEmail { get; set; } = contactEmail;
    public int Capacity { get; set; } = capacity;
    public string Department { get; set; } = department;
    public int WindowDays { get; set; } = windowDays;
    public double Weight { get; set; } = weight;
    public string Title { get; set; } = title;
    public string Description { get; set; } = description;
    public StrictPulseSurveyQuotaStatus Status { get; set; } = status;

    /// <summary>Starts a strict pulse survey quota in status planned.</summary>
    public static StrictPulseSurveyQuota NewStrictPulseSurveyQuota(string code, string name)
    {
        return new StrictPulseSurveyQuota(
            code,
            name,
            "",
            0,
            "",
            0,
            0.0,
            "",
            "",
            StrictPulseSurveyQuotaStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case StrictPulseSurveyQuotaStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(StrictPulseSurveyQuotaStatus next)
    {
        switch (Status)
        {
            case StrictPulseSurveyQuotaStatus.Planned:
                switch (next)
                {
                    case StrictPulseSurveyQuotaStatus.Completed:
                        return true;
                }
                return false;
            case StrictPulseSurveyQuotaStatus.Scheduled:
                switch (next)
                {
                    case StrictPulseSurveyQuotaStatus.Completed:
                        return true;
                }
                return false;
            case StrictPulseSurveyQuotaStatus.InProgress:
                return next == StrictPulseSurveyQuotaStatus.Completed || next == StrictPulseSurveyQuotaStatus.Cancelled;
            case StrictPulseSurveyQuotaStatus.Completed:
                switch (next)
                {
                    case StrictPulseSurveyQuotaStatus.Cancelled:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(StrictPulseSurveyQuotaStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"strict pulse survey quota {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code}: {Name}";
    }

    public double EffectiveExposure()
    {
        return Math.Round(Weight * (double)Capacity, 2);
    }

    public StrictPulseSurveyQuota Copy()
    {
        return new StrictPulseSurveyQuota(
            Code,
            Name,
            ContactEmail,
            Capacity,
            Department,
            WindowDays,
            Weight,
            Title,
            Description,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Department);
    }
}
