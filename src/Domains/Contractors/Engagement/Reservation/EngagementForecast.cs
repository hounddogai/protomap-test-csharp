namespace Acme.Workforce.Contractors.Engagement.Reservation;

using Acme.Workforce.Kit.Text;

/// <summary>Where a engagement forecast is in its life cycle.</summary>
public enum EngagementForecastStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>A engagement forecast as the engagement team keeps it.</summary>
public sealed class EngagementForecast(string code, string label, double hours, bool archived, int expiresAt, string managerId, EngagementForecastStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public double Hours { get; set; } = hours;
    public bool Archived { get; set; } = archived;
    public int ExpiresAt { get; set; } = expiresAt;
    public string ManagerId { get; set; } = managerId;
    public EngagementForecastStatus Status { get; set; } = status;

    /// <summary>Starts a engagement forecast in status new.</summary>
    public static EngagementForecast NewEngagementForecast(string code, string label)
    {
        return new EngagementForecast(code, label, 0.0, false, 0, "", EngagementForecastStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case EngagementForecastStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(EngagementForecastStatus next)
    {
        switch (Status)
        {
            case EngagementForecastStatus.New:
                switch (next)
                {
                    case EngagementForecastStatus.Running:
                        return true;
                }
                return false;
            case EngagementForecastStatus.Queued:
                switch (next)
                {
                    case EngagementForecastStatus.Done:
                        return true;
                }
                return false;
            case EngagementForecastStatus.Running:
                return next == EngagementForecastStatus.Done;
            case EngagementForecastStatus.Done:
                switch (next)
                {
                    case EngagementForecastStatus.Failed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(EngagementForecastStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"engagement forecast {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        ExpiresAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{Code} [{Label}]";
    }

    public double WeightedValue()
    {
        return Math.Round(Hours - (double)ExpiresAt, 2);
    }

    public EngagementForecast Copy()
    {
        return new EngagementForecast(Code, Label, Hours, Archived, ExpiresAt, ManagerId, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
