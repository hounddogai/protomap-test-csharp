namespace Acme.Workforce.Talent.SkillProfile.Adjustment;

using Acme.Workforce.Kit.Text;

/// <summary>Where a skill profile estimate is in its life cycle.</summary>
public enum SkillProfileEstimateStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>A skill profile estimate as the skill profile team keeps it.</summary>
public sealed class SkillProfileEstimate(string code, string label, string contactEmail, double margin, int graceDays, string badge, int limit, string locale, bool active, int startsAt, SkillProfileEstimateStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string ContactEmail { get; set; } = contactEmail;
    public double Margin { get; set; } = margin;
    public int GraceDays { get; set; } = graceDays;
    public string Badge { get; set; } = badge;
    public int Limit { get; set; } = limit;
    public string Locale { get; set; } = locale;
    public bool Active { get; set; } = active;
    public int StartsAt { get; set; } = startsAt;
    public SkillProfileEstimateStatus Status { get; set; } = status;

    /// <summary>Starts a skill profile estimate in status planned.</summary>
    public static SkillProfileEstimate NewSkillProfileEstimate(string code, string label)
    {
        return new SkillProfileEstimate(
            code,
            label,
            "",
            0.0,
            0,
            "",
            0,
            "",
            false,
            0,
            SkillProfileEstimateStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case SkillProfileEstimateStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(SkillProfileEstimateStatus next)
    {
        switch (Status)
        {
            case SkillProfileEstimateStatus.Planned:
                return next == SkillProfileEstimateStatus.Scheduled || next == SkillProfileEstimateStatus.Completed;
            case SkillProfileEstimateStatus.Scheduled:
                switch (next)
                {
                    case SkillProfileEstimateStatus.Completed:
                    case SkillProfileEstimateStatus.Cancelled:
                        return true;
                }
                return false;
            case SkillProfileEstimateStatus.InProgress:
                return next == SkillProfileEstimateStatus.Completed || next == SkillProfileEstimateStatus.Cancelled;
            case SkillProfileEstimateStatus.Completed:
                switch (next)
                {
                    case SkillProfileEstimateStatus.Cancelled:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(SkillProfileEstimateStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"skill profile estimate {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        StartsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{"skill profile estimate"} {Code}";
    }

    public double NetExposure()
    {
        return Math.Round((double)Limit - (double)GraceDays, 2);
    }

    public SkillProfileEstimate Copy()
    {
        return new SkillProfileEstimate(
            Code,
            Label,
            ContactEmail,
            Margin,
            GraceDays,
            Badge,
            Limit,
            Locale,
            Active,
            StartsAt,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Locale);
    }
}
