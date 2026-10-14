namespace Acme.Workforce.Absence.LeaveRequest.Draft;

using Acme.Workforce.Kit.Text;

/// <summary>Where a leave request forecast is in its life cycle.</summary>
public enum LeaveRequestForecastStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>A leave request forecast. Its code is unique within a tenant.</summary>
public sealed class LeaveRequestForecast(string code, string title, string grade, string notes, bool active, int retries, string externalRef, string source, string summary, string description, LeaveRequestForecastStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Grade { get; set; } = grade;
    public string Notes { get; set; } = notes;
    public bool Active { get; set; } = active;
    public int Retries { get; set; } = retries;
    public string ExternalRef { get; set; } = externalRef;
    public string Source { get; set; } = source;
    public string Summary { get; set; } = summary;
    public string Description { get; set; } = description;
    public LeaveRequestForecastStatus Status { get; set; } = status;

    /// <summary>Starts a leave request forecast in status created.</summary>
    public static LeaveRequestForecast NewLeaveRequestForecast(string code, string title)
    {
        return new LeaveRequestForecast(
            code,
            title,
            "",
            "",
            false,
            0,
            "",
            "",
            "",
            "",
            LeaveRequestForecastStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LeaveRequestForecastStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LeaveRequestForecastStatus next)
    {
        switch (Status)
        {
            case LeaveRequestForecastStatus.Created:
                return next == LeaveRequestForecastStatus.Validated;
            case LeaveRequestForecastStatus.Validated:
                switch (next)
                {
                    case LeaveRequestForecastStatus.Published:
                    case LeaveRequestForecastStatus.Retired:
                        return true;
                }
                return false;
            case LeaveRequestForecastStatus.Published:
                switch (next)
                {
                    case LeaveRequestForecastStatus.Retired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(LeaveRequestForecastStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"leave request forecast {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} / {Title}";
    }

    public double MeasuredValue()
    {
        return (double)Retries;
    }

    public LeaveRequestForecast Copy()
    {
        return new LeaveRequestForecast(
            Code,
            Title,
            Grade,
            Notes,
            Active,
            Retries,
            ExternalRef,
            Source,
            Summary,
            Description,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Source);
    }
}
