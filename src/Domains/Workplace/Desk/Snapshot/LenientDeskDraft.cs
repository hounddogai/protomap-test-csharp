namespace Acme.Workforce.Workplace.Desk.Snapshot;

using Acme.Workforce.Kit.Text;

/// <summary>Where a lenient desk draft is in its life cycle.</summary>
public enum LenientDeskDraftStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>One lenient desk draft, with what the planners need to act on it.</summary>
public sealed class LenientDeskDraft(string code, string title, int updatedAt, string department, string externalRef, LenientDeskDraftStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int UpdatedAt { get; set; } = updatedAt;
    public string Department { get; set; } = department;
    public string ExternalRef { get; set; } = externalRef;
    public LenientDeskDraftStatus Status { get; set; } = status;

    /// <summary>Starts a lenient desk draft in status new.</summary>
    public static LenientDeskDraft NewLenientDeskDraft(string code, string title)
    {
        return new LenientDeskDraft(code, title, 0, "", "", LenientDeskDraftStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LenientDeskDraftStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LenientDeskDraftStatus next)
    {
        switch (Status)
        {
            case LenientDeskDraftStatus.New:
                switch (next)
                {
                    case LenientDeskDraftStatus.Running:
                    case LenientDeskDraftStatus.Failed:
                        return true;
                }
                return false;
            case LenientDeskDraftStatus.Queued:
                switch (next)
                {
                    case LenientDeskDraftStatus.Running:
                    case LenientDeskDraftStatus.Failed:
                        return true;
                }
                return false;
            case LenientDeskDraftStatus.Running:
                switch (next)
                {
                    case LenientDeskDraftStatus.Failed:
                        return true;
                }
                return false;
            case LenientDeskDraftStatus.Done:
                switch (next)
                {
                    case LenientDeskDraftStatus.Failed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(LenientDeskDraftStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"lenient desk draft {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        UpdatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code}: {Title}";
    }

    public double MeasuredValue()
    {
        return (double)UpdatedAt;
    }

    public LenientDeskDraft Copy()
    {
        return new LenientDeskDraft(Code, Title, UpdatedAt, Department, ExternalRef, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Department);
    }
}
