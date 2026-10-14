namespace Acme.Workforce.Engagement.Feedback.FeedbackProposal;

using Acme.Workforce.Kit.Text;

/// <summary>Where a scheduled feedback forecast is in its life cycle.</summary>
public enum ScheduledFeedbackForecastStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>A scheduled feedback forecast. Its code is unique within a tenant.</summary>
public sealed class ScheduledFeedbackForecast(string code, string title, string salaryBand, double cost, string locale, string currency, bool archived, int endsAt, string employeeId, bool pinned, ScheduledFeedbackForecastStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string SalaryBand { get; set; } = salaryBand;
    public double Cost { get; set; } = cost;
    public string Locale { get; set; } = locale;
    public string Currency { get; set; } = currency;
    public bool Archived { get; set; } = archived;
    public int EndsAt { get; set; } = endsAt;
    public string EmployeeId { get; set; } = employeeId;
    public bool Pinned { get; set; } = pinned;
    public ScheduledFeedbackForecastStatus Status { get; set; } = status;

    /// <summary>Starts a scheduled feedback forecast in status draft.</summary>
    public static ScheduledFeedbackForecast NewScheduledFeedbackForecast(string code, string title)
    {
        return new ScheduledFeedbackForecast(
            code,
            title,
            "",
            0.0,
            "",
            "",
            false,
            0,
            "",
            false,
            ScheduledFeedbackForecastStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ScheduledFeedbackForecastStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ScheduledFeedbackForecastStatus next)
    {
        switch (Status)
        {
            case ScheduledFeedbackForecastStatus.Draft:
                return next == ScheduledFeedbackForecastStatus.Suspended;
            case ScheduledFeedbackForecastStatus.Pending:
                return next == ScheduledFeedbackForecastStatus.Active || next == ScheduledFeedbackForecastStatus.Suspended;
            case ScheduledFeedbackForecastStatus.Active:
                switch (next)
                {
                    case ScheduledFeedbackForecastStatus.Suspended:
                    case ScheduledFeedbackForecastStatus.Closed:
                        return true;
                }
                return false;
            case ScheduledFeedbackForecastStatus.Suspended:
                switch (next)
                {
                    case ScheduledFeedbackForecastStatus.Closed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(ScheduledFeedbackForecastStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"scheduled feedback forecast {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        EndsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} ({Title})";
    }

    public double ProjectedExposure()
    {
        return Math.Round((double)EndsAt + Cost, 2);
    }

    public ScheduledFeedbackForecast Copy()
    {
        return new ScheduledFeedbackForecast(
            Code,
            Title,
            SalaryBand,
            Cost,
            Locale,
            Currency,
            Archived,
            EndsAt,
            EmployeeId,
            Pinned,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Locale);
    }
}
