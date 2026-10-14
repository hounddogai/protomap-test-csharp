namespace Acme.Workforce.Talent.Pool.Draft;

using Acme.Workforce.Kit.Text;

/// <summary>Where a lenient pool draft is in its life cycle.</summary>
public enum LenientPoolDraftStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>The record of a lenient pool draft and its current status.</summary>
public sealed class LenientPoolDraft(string code, string title, string channel, string managerId, int sequence, bool pinned, double hours, LenientPoolDraftStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Channel { get; set; } = channel;
    public string ManagerId { get; set; } = managerId;
    public int Sequence { get; set; } = sequence;
    public bool Pinned { get; set; } = pinned;
    public double Hours { get; set; } = hours;
    public LenientPoolDraftStatus Status { get; set; } = status;

    /// <summary>Starts a lenient pool draft in status new.</summary>
    public static LenientPoolDraft NewLenientPoolDraft(string code, string title)
    {
        return new LenientPoolDraft(code, title, "", "", 0, false, 0.0, LenientPoolDraftStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LenientPoolDraftStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LenientPoolDraftStatus next)
    {
        switch (Status)
        {
            case LenientPoolDraftStatus.New:
                switch (next)
                {
                    case LenientPoolDraftStatus.Failed:
                        return true;
                }
                return false;
            case LenientPoolDraftStatus.Queued:
                switch (next)
                {
                    case LenientPoolDraftStatus.Done:
                    case LenientPoolDraftStatus.Failed:
                        return true;
                }
                return false;
            case LenientPoolDraftStatus.Running:
                switch (next)
                {
                    case LenientPoolDraftStatus.Done:
                    case LenientPoolDraftStatus.Failed:
                        return true;
                }
                return false;
            case LenientPoolDraftStatus.Done:
                return next == LenientPoolDraftStatus.Failed;
        }
        return false;
    }

    public void MoveTo(LenientPoolDraftStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"lenient pool draft {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"lenient pool draft"} {Code}";
    }

    public double AdjustedTotal()
    {
        return Math.Round(Hours - (double)Sequence, 2);
    }

    public LenientPoolDraft Copy()
    {
        return new LenientPoolDraft(Code, Title, Channel, ManagerId, Sequence, Pinned, Hours, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Channel);
    }
}
