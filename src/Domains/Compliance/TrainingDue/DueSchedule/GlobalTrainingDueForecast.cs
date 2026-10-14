namespace Acme.Workforce.Compliance.TrainingDue.DueSchedule;

using Acme.Workforce.Kit.Text;

/// <summary>Where a global training due forecast is in its life cycle.</summary>
public enum GlobalTrainingDueForecastStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>The record of a global training due forecast and its current status.</summary>
public sealed class GlobalTrainingDueForecast(string code, string title, double ceiling, string timezone, double rate, int priority, double rating, GlobalTrainingDueForecastStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public double Ceiling { get; set; } = ceiling;
    public string Timezone { get; set; } = timezone;
    public double Rate { get; set; } = rate;
    public int Priority { get; set; } = priority;
    public double Rating { get; set; } = rating;
    public GlobalTrainingDueForecastStatus Status { get; set; } = status;

    /// <summary>Starts a global training due forecast in status proposed.</summary>
    public static GlobalTrainingDueForecast NewGlobalTrainingDueForecast(string code, string title)
    {
        return new GlobalTrainingDueForecast(
            code,
            title,
            0.0,
            "",
            0.0,
            0,
            0.0,
            GlobalTrainingDueForecastStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case GlobalTrainingDueForecastStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(GlobalTrainingDueForecastStatus next)
    {
        switch (Status)
        {
            case GlobalTrainingDueForecastStatus.Proposed:
                return next == GlobalTrainingDueForecastStatus.Approved;
            case GlobalTrainingDueForecastStatus.Approved:
                return next == GlobalTrainingDueForecastStatus.Rejected || next == GlobalTrainingDueForecastStatus.Withdrawn;
            case GlobalTrainingDueForecastStatus.Rejected:
                switch (next)
                {
                    case GlobalTrainingDueForecastStatus.Withdrawn:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(GlobalTrainingDueForecastStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"global training due forecast {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code}: {Title}";
    }

    public double ProjectedLoad()
    {
        return Math.Round(Rate * (double)Priority, 2);
    }

    public GlobalTrainingDueForecast Copy()
    {
        return new GlobalTrainingDueForecast(Code, Title, Ceiling, Timezone, Rate, Priority, Rating, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
