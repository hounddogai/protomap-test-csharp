namespace Acme.Workforce.Engagement.Recognition.Score;

using Acme.Workforce.Kit.Text;

/// <summary>Where a recognition score is in its life cycle.</summary>
public enum RecognitionScoreStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>One recognition score, with what the planners need to act on it.</summary>
public sealed class RecognitionScore(string code, string label, bool archived, string employeeId, int durationMinutes, double rate, RecognitionScoreStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public bool Archived { get; set; } = archived;
    public string EmployeeId { get; set; } = employeeId;
    public int DurationMinutes { get; set; } = durationMinutes;
    public double Rate { get; set; } = rate;
    public RecognitionScoreStatus Status { get; set; } = status;

    /// <summary>Starts a recognition score in status new.</summary>
    public static RecognitionScore NewRecognitionScore(string code, string label)
    {
        return new RecognitionScore(code, label, false, "", 0, 0.0, RecognitionScoreStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RecognitionScoreStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RecognitionScoreStatus next)
    {
        switch (Status)
        {
            case RecognitionScoreStatus.New:
                return next == RecognitionScoreStatus.Queued || next == RecognitionScoreStatus.Done;
            case RecognitionScoreStatus.Queued:
                switch (next)
                {
                    case RecognitionScoreStatus.Running:
                    case RecognitionScoreStatus.Failed:
                        return true;
                }
                return false;
            case RecognitionScoreStatus.Running:
                return next == RecognitionScoreStatus.Done || next == RecognitionScoreStatus.Failed;
            case RecognitionScoreStatus.Done:
                return next == RecognitionScoreStatus.Failed;
        }
        return false;
    }

    public void MoveTo(RecognitionScoreStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"recognition score {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} [{Label}]";
    }

    public double EffectiveExposure()
    {
        return Math.Round(Rate - (double)DurationMinutes, 2);
    }

    public RecognitionScore Copy()
    {
        return new RecognitionScore(Code, Label, Archived, EmployeeId, DurationMinutes, Rate, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
