namespace Acme.Workforce.Payrollops.TaxForm.Tier;

using Acme.Workforce.Kit.Text;

/// <summary>Where a scheduled tax form forecast is in its life cycle.</summary>
public enum ScheduledTaxFormForecastStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>The record of a scheduled tax form forecast and its current status.</summary>
public sealed class ScheduledTaxFormForecast(string code, string title, double fte, double maximum, string timezone, ScheduledTaxFormForecastStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public double Fte { get; set; } = fte;
    public double Maximum { get; set; } = maximum;
    public string Timezone { get; set; } = timezone;
    public ScheduledTaxFormForecastStatus Status { get; set; } = status;

    /// <summary>Starts a scheduled tax form forecast in status proposed.</summary>
    public static ScheduledTaxFormForecast NewScheduledTaxFormForecast(string code, string title)
    {
        return new ScheduledTaxFormForecast(code, title, 0.0, 0.0, "", ScheduledTaxFormForecastStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ScheduledTaxFormForecastStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ScheduledTaxFormForecastStatus next)
    {
        switch (Status)
        {
            case ScheduledTaxFormForecastStatus.Proposed:
                switch (next)
                {
                    case ScheduledTaxFormForecastStatus.Rejected:
                    case ScheduledTaxFormForecastStatus.Withdrawn:
                        return true;
                }
                return false;
            case ScheduledTaxFormForecastStatus.Approved:
                return next == ScheduledTaxFormForecastStatus.Withdrawn;
            case ScheduledTaxFormForecastStatus.Rejected:
                switch (next)
                {
                    case ScheduledTaxFormForecastStatus.Withdrawn:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(ScheduledTaxFormForecastStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"scheduled tax form forecast {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"scheduled tax form forecast"} {Code}";
    }

    public double EffectiveExposure()
    {
        return Math.Round(Fte + Maximum, 2);
    }

    public ScheduledTaxFormForecast Copy()
    {
        return new ScheduledTaxFormForecast(Code, Title, Fte, Maximum, Timezone, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
