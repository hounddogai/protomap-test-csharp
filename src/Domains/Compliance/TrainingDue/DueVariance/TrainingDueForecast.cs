namespace Acme.Workforce.Compliance.TrainingDue.DueVariance;

using Acme.Workforce.Kit.Text;

/// <summary>Where a training due forecast is in its life cycle.</summary>
public enum TrainingDueForecastStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>A training due forecast as the training due team keeps it.</summary>
public sealed class TrainingDueForecast(string code, string label, bool approved, double rate, bool archived, string skill, double hours, int updatedAt, double weight, double budget, TrainingDueForecastStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public bool Approved { get; set; } = approved;
    public double Rate { get; set; } = rate;
    public bool Archived { get; set; } = archived;
    public string Skill { get; set; } = skill;
    public double Hours { get; set; } = hours;
    public int UpdatedAt { get; set; } = updatedAt;
    public double Weight { get; set; } = weight;
    public double Budget { get; set; } = budget;
    public TrainingDueForecastStatus Status { get; set; } = status;

    /// <summary>Starts a training due forecast in status planned.</summary>
    public static TrainingDueForecast NewTrainingDueForecast(string code, string label)
    {
        return new TrainingDueForecast(
            code,
            label,
            false,
            0.0,
            false,
            "",
            0.0,
            0,
            0.0,
            0.0,
            TrainingDueForecastStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case TrainingDueForecastStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(TrainingDueForecastStatus next)
    {
        switch (Status)
        {
            case TrainingDueForecastStatus.Planned:
                return next == TrainingDueForecastStatus.InProgress || next == TrainingDueForecastStatus.Completed;
            case TrainingDueForecastStatus.Scheduled:
                return next == TrainingDueForecastStatus.InProgress || next == TrainingDueForecastStatus.Cancelled;
            case TrainingDueForecastStatus.InProgress:
                return next == TrainingDueForecastStatus.Cancelled;
            case TrainingDueForecastStatus.Completed:
                return next == TrainingDueForecastStatus.Cancelled;
        }
        return false;
    }

    public void MoveTo(TrainingDueForecastStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"training due forecast {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        UpdatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{"training due forecast"} {Code}";
    }

    public double NetValue()
    {
        return Math.Round((double)UpdatedAt * Hours, 2);
    }

    public TrainingDueForecast Copy()
    {
        return new TrainingDueForecast(
            Code,
            Label,
            Approved,
            Rate,
            Archived,
            Skill,
            Hours,
            UpdatedAt,
            Weight,
            Budget,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
