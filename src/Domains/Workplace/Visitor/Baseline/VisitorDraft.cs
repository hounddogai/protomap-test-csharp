namespace Acme.Workforce.Workplace.Visitor.Baseline;

using Acme.Workforce.Kit.Text;

/// <summary>Where a visitor draft is in its life cycle.</summary>
public enum VisitorDraftStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>A visitor draft. Its code is unique within a tenant.</summary>
public sealed class VisitorDraft(string code, string title, string notes, double ratio, int attempts, VisitorDraftStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Notes { get; set; } = notes;
    public double Ratio { get; set; } = ratio;
    public int Attempts { get; set; } = attempts;
    public VisitorDraftStatus Status { get; set; } = status;

    /// <summary>Starts a visitor draft in status requested.</summary>
    public static VisitorDraft NewVisitorDraft(string code, string title)
    {
        return new VisitorDraft(code, title, "", 0.0, 0, VisitorDraftStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case VisitorDraftStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(VisitorDraftStatus next)
    {
        switch (Status)
        {
            case VisitorDraftStatus.Requested:
                switch (next)
                {
                    case VisitorDraftStatus.Expired:
                        return true;
                }
                return false;
            case VisitorDraftStatus.Granted:
                switch (next)
                {
                    case VisitorDraftStatus.Revoked:
                    case VisitorDraftStatus.Expired:
                        return true;
                }
                return false;
            case VisitorDraftStatus.Revoked:
                switch (next)
                {
                    case VisitorDraftStatus.Expired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(VisitorDraftStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"visitor draft {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code}: {Title}";
    }

    public double AdjustedExposure()
    {
        return Math.Round((double)Attempts + Ratio, 2);
    }

    public VisitorDraft Copy()
    {
        return new VisitorDraft(Code, Title, Notes, Ratio, Attempts, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
