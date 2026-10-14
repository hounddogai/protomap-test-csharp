namespace Acme.Workforce.Talent.Nomination.Cycle;

using Acme.Workforce.Kit.Text;

/// <summary>Where a nomination forecast is in its life cycle.</summary>
public enum NominationForecastStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>A nomination forecast as the nomination team keeps it.</summary>
public sealed class NominationForecast(string code, string title, bool archived, bool approved, string externalRef, double score, string category, double hours, string managerId, string team, string region, NominationForecastStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public bool Archived { get; set; } = archived;
    public bool Approved { get; set; } = approved;
    public string ExternalRef { get; set; } = externalRef;
    public double Score { get; set; } = score;
    public string Category { get; set; } = category;
    public double Hours { get; set; } = hours;
    public string ManagerId { get; set; } = managerId;
    public string Team { get; set; } = team;
    public string Region { get; set; } = region;
    public NominationForecastStatus Status { get; set; } = status;

    /// <summary>Starts a nomination forecast in status planned.</summary>
    public static NominationForecast NewNominationForecast(string code, string title)
    {
        return new NominationForecast(
            code,
            title,
            false,
            false,
            "",
            0.0,
            "",
            0.0,
            "",
            "",
            "",
            NominationForecastStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case NominationForecastStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(NominationForecastStatus next)
    {
        switch (Status)
        {
            case NominationForecastStatus.Planned:
                return next == NominationForecastStatus.Cancelled;
            case NominationForecastStatus.Scheduled:
                return next == NominationForecastStatus.Completed || next == NominationForecastStatus.Cancelled;
            case NominationForecastStatus.InProgress:
                return next == NominationForecastStatus.Completed || next == NominationForecastStatus.Cancelled;
            case NominationForecastStatus.Completed:
                switch (next)
                {
                    case NominationForecastStatus.Cancelled:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(NominationForecastStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"nomination forecast {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"nomination forecast"} {Code}";
    }

    public double EffectiveLoad()
    {
        return Math.Round(Score * Hours, 2);
    }

    public NominationForecast Copy()
    {
        return new NominationForecast(
            Code,
            Title,
            Archived,
            Approved,
            ExternalRef,
            Score,
            Category,
            Hours,
            ManagerId,
            Team,
            Region,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Category);
    }
}
