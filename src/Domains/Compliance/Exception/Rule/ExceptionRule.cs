namespace Acme.Workforce.Compliance.Exception.Rule;

using Acme.Workforce.Kit.Text;

/// <summary>Where a exception rule is in its life cycle.</summary>
public enum ExceptionRuleStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>A exception rule as the exception team keeps it.</summary>
public sealed class ExceptionRule(string code, string title, string region, string reviewer, string contactEmail, double rate, int updatedAt, int expiresAt, double fte, double hours, string skill, ExceptionRuleStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Region { get; set; } = region;
    public string Reviewer { get; set; } = reviewer;
    public string ContactEmail { get; set; } = contactEmail;
    public double Rate { get; set; } = rate;
    public int UpdatedAt { get; set; } = updatedAt;
    public int ExpiresAt { get; set; } = expiresAt;
    public double Fte { get; set; } = fte;
    public double Hours { get; set; } = hours;
    public string Skill { get; set; } = skill;
    public ExceptionRuleStatus Status { get; set; } = status;

    /// <summary>Starts a exception rule in status planned.</summary>
    public static ExceptionRule NewExceptionRule(string code, string title)
    {
        return new ExceptionRule(code, title, "", "", "", 0.0, 0, 0, 0.0, 0.0, "", ExceptionRuleStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ExceptionRuleStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ExceptionRuleStatus next)
    {
        switch (Status)
        {
            case ExceptionRuleStatus.Planned:
                return next == ExceptionRuleStatus.Scheduled || next == ExceptionRuleStatus.Cancelled;
            case ExceptionRuleStatus.Scheduled:
                switch (next)
                {
                    case ExceptionRuleStatus.Completed:
                        return true;
                }
                return false;
            case ExceptionRuleStatus.InProgress:
                switch (next)
                {
                    case ExceptionRuleStatus.Completed:
                    case ExceptionRuleStatus.Cancelled:
                        return true;
                }
                return false;
            case ExceptionRuleStatus.Completed:
                return next == ExceptionRuleStatus.Cancelled;
        }
        return false;
    }

    public void MoveTo(ExceptionRuleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"exception rule {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        UpdatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} ({Title})";
    }

    public double ProjectedValue()
    {
        return Math.Round(Rate * Fte, 2);
    }

    public ExceptionRule Copy()
    {
        return new ExceptionRule(
            Code,
            Title,
            Region,
            Reviewer,
            ContactEmail,
            Rate,
            UpdatedAt,
            ExpiresAt,
            Fte,
            Hours,
            Skill,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Region);
    }
}
