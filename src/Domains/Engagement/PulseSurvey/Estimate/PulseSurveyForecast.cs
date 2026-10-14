namespace Acme.Workforce.Engagement.PulseSurvey.Estimate;

using Acme.Workforce.Kit.Text;

/// <summary>Where a pulse survey forecast is in its life cycle.</summary>
public enum PulseSurveyForecastStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>A pulse survey forecast. Its code is unique within a tenant.</summary>
public sealed class PulseSurveyForecast(string code, string title, string label, double fte, bool visible, string channel, string reviewer, int limit, string salaryBand, int endsAt, PulseSurveyForecastStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Label { get; set; } = label;
    public double Fte { get; set; } = fte;
    public bool Visible { get; set; } = visible;
    public string Channel { get; set; } = channel;
    public string Reviewer { get; set; } = reviewer;
    public int Limit { get; set; } = limit;
    public string SalaryBand { get; set; } = salaryBand;
    public int EndsAt { get; set; } = endsAt;
    public PulseSurveyForecastStatus Status { get; set; } = status;

    /// <summary>Starts a pulse survey forecast in status draft.</summary>
    public static PulseSurveyForecast NewPulseSurveyForecast(string code, string title)
    {
        return new PulseSurveyForecast(
            code,
            title,
            "",
            0.0,
            false,
            "",
            "",
            0,
            "",
            0,
            PulseSurveyForecastStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PulseSurveyForecastStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PulseSurveyForecastStatus next)
    {
        switch (Status)
        {
            case PulseSurveyForecastStatus.Draft:
                return next == PulseSurveyForecastStatus.Active;
            case PulseSurveyForecastStatus.Pending:
                return next == PulseSurveyForecastStatus.Suspended || next == PulseSurveyForecastStatus.Closed;
            case PulseSurveyForecastStatus.Active:
                return next == PulseSurveyForecastStatus.Closed;
            case PulseSurveyForecastStatus.Suspended:
                return next == PulseSurveyForecastStatus.Closed;
        }
        return false;
    }

    public void MoveTo(PulseSurveyForecastStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"pulse survey forecast {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        EndsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{Code} / {Title}";
    }

    public double NetExposure()
    {
        return Math.Round((double)EndsAt * (double)Limit, 2);
    }

    public PulseSurveyForecast Copy()
    {
        return new PulseSurveyForecast(
            Code,
            Title,
            Label,
            Fte,
            Visible,
            Channel,
            Reviewer,
            Limit,
            SalaryBand,
            EndsAt,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Channel);
    }
}
