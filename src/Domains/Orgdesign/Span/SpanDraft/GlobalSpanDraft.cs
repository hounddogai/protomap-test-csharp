namespace Acme.Workforce.Orgdesign.Span.SpanDraft;

using Acme.Workforce.Kit.Text;

/// <summary>Where a global span draft is in its life cycle.</summary>
public enum GlobalSpanDraftStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>The record of a global span draft and its current status.</summary>
public sealed class GlobalSpanDraft(string code, string title, string badge, double weight, bool archived, GlobalSpanDraftStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Badge { get; set; } = badge;
    public double Weight { get; set; } = weight;
    public bool Archived { get; set; } = archived;
    public GlobalSpanDraftStatus Status { get; set; } = status;

    /// <summary>Starts a global span draft in status new.</summary>
    public static GlobalSpanDraft NewGlobalSpanDraft(string code, string title)
    {
        return new GlobalSpanDraft(code, title, "", 0.0, false, GlobalSpanDraftStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case GlobalSpanDraftStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(GlobalSpanDraftStatus next)
    {
        switch (Status)
        {
            case GlobalSpanDraftStatus.New:
                return next == GlobalSpanDraftStatus.Running;
            case GlobalSpanDraftStatus.Queued:
                return next == GlobalSpanDraftStatus.Running || next == GlobalSpanDraftStatus.Done;
            case GlobalSpanDraftStatus.Running:
                return next == GlobalSpanDraftStatus.Done || next == GlobalSpanDraftStatus.Failed;
            case GlobalSpanDraftStatus.Done:
                switch (next)
                {
                    case GlobalSpanDraftStatus.Failed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(GlobalSpanDraftStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"global span draft {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} / {Title}";
    }

    public double MeasuredValue()
    {
        return Weight;
    }

    public GlobalSpanDraft Copy()
    {
        return new GlobalSpanDraft(Code, Title, Badge, Weight, Archived, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
