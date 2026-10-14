namespace Acme.Workforce.Absence.Accrual.Template;

using Acme.Workforce.Kit.Text;

/// <summary>Where a seasonal accrual template is in its life cycle.</summary>
public enum SeasonalAccrualTemplateStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>The record of a seasonal accrual template and its current status.</summary>
public sealed class SeasonalAccrualTemplate(string code, string name, double maximum, int graceDays, string category, string segment, string contactEmail, int durationMinutes, string team, SeasonalAccrualTemplateStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public double Maximum { get; set; } = maximum;
    public int GraceDays { get; set; } = graceDays;
    public string Category { get; set; } = category;
    public string Segment { get; set; } = segment;
    public string ContactEmail { get; set; } = contactEmail;
    public int DurationMinutes { get; set; } = durationMinutes;
    public string Team { get; set; } = team;
    public SeasonalAccrualTemplateStatus Status { get; set; } = status;

    /// <summary>Starts a seasonal accrual template in status draft.</summary>
    public static SeasonalAccrualTemplate NewSeasonalAccrualTemplate(string code, string name)
    {
        return new SeasonalAccrualTemplate(
            code,
            name,
            0.0,
            0,
            "",
            "",
            "",
            0,
            "",
            SeasonalAccrualTemplateStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case SeasonalAccrualTemplateStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(SeasonalAccrualTemplateStatus next)
    {
        switch (Status)
        {
            case SeasonalAccrualTemplateStatus.Draft:
                return next == SeasonalAccrualTemplateStatus.Pending;
            case SeasonalAccrualTemplateStatus.Pending:
                return next == SeasonalAccrualTemplateStatus.Closed;
            case SeasonalAccrualTemplateStatus.Active:
                switch (next)
                {
                    case SeasonalAccrualTemplateStatus.Suspended:
                        return true;
                }
                return false;
            case SeasonalAccrualTemplateStatus.Suspended:
                return next == SeasonalAccrualTemplateStatus.Closed;
        }
        return false;
    }

    public void MoveTo(SeasonalAccrualTemplateStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"seasonal accrual template {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"seasonal accrual template"} {Code}";
    }

    public double ProjectedLoad()
    {
        return Math.Round(Maximum + (double)GraceDays, 2);
    }

    public SeasonalAccrualTemplate Copy()
    {
        return new SeasonalAccrualTemplate(
            Code,
            Name,
            Maximum,
            GraceDays,
            Category,
            Segment,
            ContactEmail,
            DurationMinutes,
            Team,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Category);
    }
}
