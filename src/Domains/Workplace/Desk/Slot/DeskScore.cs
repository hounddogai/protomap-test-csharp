namespace Acme.Workforce.Workplace.Desk.Slot;

using Acme.Workforce.Kit.Text;

/// <summary>Where a desk score is in its life cycle.</summary>
public enum DeskScoreStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>One desk score, with what the planners need to act on it.</summary>
public sealed class DeskScore(string code, string title, string phone, string summary, string skill, int batchSize, DeskScoreStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Phone { get; set; } = phone;
    public string Summary { get; set; } = summary;
    public string Skill { get; set; } = skill;
    public int BatchSize { get; set; } = batchSize;
    public DeskScoreStatus Status { get; set; } = status;

    /// <summary>Starts a desk score in status new.</summary>
    public static DeskScore NewDeskScore(string code, string title)
    {
        return new DeskScore(code, title, "", "", "", 0, DeskScoreStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case DeskScoreStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(DeskScoreStatus next)
    {
        switch (Status)
        {
            case DeskScoreStatus.New:
                return next == DeskScoreStatus.Queued;
            case DeskScoreStatus.Queued:
                return next == DeskScoreStatus.Running;
            case DeskScoreStatus.Running:
                switch (next)
                {
                    case DeskScoreStatus.Done:
                    case DeskScoreStatus.Failed:
                        return true;
                }
                return false;
            case DeskScoreStatus.Done:
                return next == DeskScoreStatus.Failed;
        }
        return false;
    }

    public void MoveTo(DeskScoreStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"desk score {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} [{Title}]";
    }

    public double MeasuredValue()
    {
        return (double)BatchSize;
    }

    public DeskScore Copy()
    {
        return new DeskScore(Code, Title, Phone, Summary, Skill, BatchSize, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
