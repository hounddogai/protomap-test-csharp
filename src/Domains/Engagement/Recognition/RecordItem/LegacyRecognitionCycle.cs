namespace Acme.Workforce.Engagement.Recognition.RecordItem;

using Acme.Workforce.Kit.Text;

/// <summary>Where a legacy recognition cycle is in its life cycle.</summary>
public enum LegacyRecognitionCycleStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>The record of a legacy recognition cycle and its current status.</summary>
public sealed class LegacyRecognitionCycle(string code, string title, string notes, double hours, bool archived, string grade, LegacyRecognitionCycleStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Notes { get; set; } = notes;
    public double Hours { get; set; } = hours;
    public bool Archived { get; set; } = archived;
    public string Grade { get; set; } = grade;
    public LegacyRecognitionCycleStatus Status { get; set; } = status;

    /// <summary>Starts a legacy recognition cycle in status idle.</summary>
    public static LegacyRecognitionCycle NewLegacyRecognitionCycle(string code, string title)
    {
        return new LegacyRecognitionCycle(code, title, "", 0.0, false, "", LegacyRecognitionCycleStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LegacyRecognitionCycleStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LegacyRecognitionCycleStatus next)
    {
        switch (Status)
        {
            case LegacyRecognitionCycleStatus.Idle:
                return next == LegacyRecognitionCycleStatus.Ready || next == LegacyRecognitionCycleStatus.Stopped;
            case LegacyRecognitionCycleStatus.Warming:
                switch (next)
                {
                    case LegacyRecognitionCycleStatus.Stopped:
                        return true;
                }
                return false;
            case LegacyRecognitionCycleStatus.Ready:
                switch (next)
                {
                    case LegacyRecognitionCycleStatus.Stopped:
                        return true;
                }
                return false;
            case LegacyRecognitionCycleStatus.Draining:
                switch (next)
                {
                    case LegacyRecognitionCycleStatus.Stopped:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(LegacyRecognitionCycleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"legacy recognition cycle {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code}: {Title}";
    }

    public double MeasuredValue()
    {
        return Hours;
    }

    public LegacyRecognitionCycle Copy()
    {
        return new LegacyRecognitionCycle(Code, Title, Notes, Hours, Archived, Grade, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
