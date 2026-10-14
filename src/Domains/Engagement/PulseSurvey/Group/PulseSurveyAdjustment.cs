namespace Acme.Workforce.Engagement.PulseSurvey.Group;

using Acme.Workforce.Kit.Text;

/// <summary>Where a pulse survey adjustment is in its life cycle.</summary>
public enum PulseSurveyAdjustmentStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>A pulse survey adjustment as the pulse survey team keeps it.</summary>
public sealed class PulseSurveyAdjustment(string code, string name, string managerId, double rating, int attempts, PulseSurveyAdjustmentStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string ManagerId { get; set; } = managerId;
    public double Rating { get; set; } = rating;
    public int Attempts { get; set; } = attempts;
    public PulseSurveyAdjustmentStatus Status { get; set; } = status;

    /// <summary>Starts a pulse survey adjustment in status planned.</summary>
    public static PulseSurveyAdjustment NewPulseSurveyAdjustment(string code, string name)
    {
        return new PulseSurveyAdjustment(code, name, "", 0.0, 0, PulseSurveyAdjustmentStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PulseSurveyAdjustmentStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PulseSurveyAdjustmentStatus next)
    {
        switch (Status)
        {
            case PulseSurveyAdjustmentStatus.Planned:
                switch (next)
                {
                    case PulseSurveyAdjustmentStatus.Scheduled:
                        return true;
                }
                return false;
            case PulseSurveyAdjustmentStatus.Scheduled:
                switch (next)
                {
                    case PulseSurveyAdjustmentStatus.InProgress:
                    case PulseSurveyAdjustmentStatus.Cancelled:
                        return true;
                }
                return false;
            case PulseSurveyAdjustmentStatus.InProgress:
                return next == PulseSurveyAdjustmentStatus.Cancelled;
            case PulseSurveyAdjustmentStatus.Completed:
                switch (next)
                {
                    case PulseSurveyAdjustmentStatus.Cancelled:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(PulseSurveyAdjustmentStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"pulse survey adjustment {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code}: {Name}";
    }

    public double NetVolume()
    {
        return Math.Round((double)Attempts + Rating, 2);
    }

    public PulseSurveyAdjustment Copy()
    {
        return new PulseSurveyAdjustment(Code, Name, ManagerId, Rating, Attempts, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
