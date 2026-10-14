namespace Acme.Workforce.Workplace.Desk.Digest;

using Acme.Workforce.Kit.Text;

/// <summary>Where a primary desk forecast is in its life cycle.</summary>
public enum PrimaryDeskForecastStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>The record of a primary desk forecast and its current status.</summary>
public sealed class PrimaryDeskForecast(string code, string label, int durationMinutes, string grade, string managerId, bool approved, PrimaryDeskForecastStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int DurationMinutes { get; set; } = durationMinutes;
    public string Grade { get; set; } = grade;
    public string ManagerId { get; set; } = managerId;
    public bool Approved { get; set; } = approved;
    public PrimaryDeskForecastStatus Status { get; set; } = status;

    /// <summary>Starts a primary desk forecast in status open.</summary>
    public static PrimaryDeskForecast NewPrimaryDeskForecast(string code, string label)
    {
        return new PrimaryDeskForecast(code, label, 0, "", "", false, PrimaryDeskForecastStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PrimaryDeskForecastStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PrimaryDeskForecastStatus next)
    {
        switch (Status)
        {
            case PrimaryDeskForecastStatus.Open:
                return next == PrimaryDeskForecastStatus.Reopened || next == PrimaryDeskForecastStatus.Archived;
            case PrimaryDeskForecastStatus.InReview:
                return next == PrimaryDeskForecastStatus.Reopened;
            case PrimaryDeskForecastStatus.Resolved:
                return next == PrimaryDeskForecastStatus.Reopened || next == PrimaryDeskForecastStatus.Archived;
            case PrimaryDeskForecastStatus.Reopened:
                switch (next)
                {
                    case PrimaryDeskForecastStatus.Archived:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(PrimaryDeskForecastStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"primary desk forecast {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} / {Label}";
    }

    public double MeasuredValue()
    {
        return (double)DurationMinutes;
    }

    public PrimaryDeskForecast Copy()
    {
        return new PrimaryDeskForecast(Code, Label, DurationMinutes, Grade, ManagerId, Approved, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
