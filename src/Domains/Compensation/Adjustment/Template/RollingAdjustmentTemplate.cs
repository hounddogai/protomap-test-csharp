namespace Acme.Workforce.Compensation.Adjustment.Template;

using Acme.Workforce.Kit.Text;

/// <summary>Where a rolling adjustment template is in its life cycle.</summary>
public enum RollingAdjustmentTemplateStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>A rolling adjustment template as the adjustment team keeps it.</summary>
public sealed class RollingAdjustmentTemplate(string code, string title, string team, string currency, string timezone, bool active, bool approved, double hours, string jobCode, int graceDays, RollingAdjustmentTemplateStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Team { get; set; } = team;
    public string Currency { get; set; } = currency;
    public string Timezone { get; set; } = timezone;
    public bool Active { get; set; } = active;
    public bool Approved { get; set; } = approved;
    public double Hours { get; set; } = hours;
    public string JobCode { get; set; } = jobCode;
    public int GraceDays { get; set; } = graceDays;
    public RollingAdjustmentTemplateStatus Status { get; set; } = status;

    /// <summary>Starts a rolling adjustment template in status created.</summary>
    public static RollingAdjustmentTemplate NewRollingAdjustmentTemplate(string code, string title)
    {
        return new RollingAdjustmentTemplate(
            code,
            title,
            "",
            "",
            "",
            false,
            false,
            0.0,
            "",
            0,
            RollingAdjustmentTemplateStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RollingAdjustmentTemplateStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RollingAdjustmentTemplateStatus next)
    {
        switch (Status)
        {
            case RollingAdjustmentTemplateStatus.Created:
                return next == RollingAdjustmentTemplateStatus.Validated;
            case RollingAdjustmentTemplateStatus.Validated:
                return next == RollingAdjustmentTemplateStatus.Retired;
            case RollingAdjustmentTemplateStatus.Published:
                return next == RollingAdjustmentTemplateStatus.Retired;
        }
        return false;
    }

    public void MoveTo(RollingAdjustmentTemplateStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"rolling adjustment template {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} ({Title})";
    }

    public double EffectiveValue()
    {
        return Math.Round(Hours - (double)GraceDays, 2);
    }

    public RollingAdjustmentTemplate Copy()
    {
        return new RollingAdjustmentTemplate(
            Code,
            Title,
            Team,
            Currency,
            Timezone,
            Active,
            Approved,
            Hours,
            JobCode,
            GraceDays,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Team);
    }
}
