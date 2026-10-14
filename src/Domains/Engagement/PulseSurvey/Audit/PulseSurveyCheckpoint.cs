namespace Acme.Workforce.Engagement.PulseSurvey.Audit;

using Acme.Workforce.Kit.Text;

/// <summary>Where a pulse survey checkpoint is in its life cycle.</summary>
public enum PulseSurveyCheckpointStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>One pulse survey checkpoint, with what the planners need to act on it.</summary>
public sealed class PulseSurveyCheckpoint(string code, string label, string grade, string jobCode, string title, string location, int endsAt, double budget, double floorValue, PulseSurveyCheckpointStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Grade { get; set; } = grade;
    public string JobCode { get; set; } = jobCode;
    public string Title { get; set; } = title;
    public string Location { get; set; } = location;
    public int EndsAt { get; set; } = endsAt;
    public double Budget { get; set; } = budget;
    public double FloorValue { get; set; } = floorValue;
    public PulseSurveyCheckpointStatus Status { get; set; } = status;

    /// <summary>Starts a pulse survey checkpoint in status open.</summary>
    public static PulseSurveyCheckpoint NewPulseSurveyCheckpoint(string code, string label)
    {
        return new PulseSurveyCheckpoint(code, label, "", "", "", "", 0, 0.0, 0.0, PulseSurveyCheckpointStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PulseSurveyCheckpointStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PulseSurveyCheckpointStatus next)
    {
        switch (Status)
        {
            case PulseSurveyCheckpointStatus.Open:
                switch (next)
                {
                    case PulseSurveyCheckpointStatus.InReview:
                        return true;
                }
                return false;
            case PulseSurveyCheckpointStatus.InReview:
                switch (next)
                {
                    case PulseSurveyCheckpointStatus.Resolved:
                        return true;
                }
                return false;
            case PulseSurveyCheckpointStatus.Resolved:
                switch (next)
                {
                    case PulseSurveyCheckpointStatus.Reopened:
                        return true;
                }
                return false;
            case PulseSurveyCheckpointStatus.Reopened:
                switch (next)
                {
                    case PulseSurveyCheckpointStatus.Archived:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(PulseSurveyCheckpointStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"pulse survey checkpoint {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        EndsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{Code}: {Label}";
    }

    public double AdjustedLoad()
    {
        return Math.Round((double)EndsAt + FloorValue, 2);
    }

    public PulseSurveyCheckpoint Copy()
    {
        return new PulseSurveyCheckpoint(
            Code,
            Label,
            Grade,
            JobCode,
            Title,
            Location,
            EndsAt,
            Budget,
            FloorValue,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
