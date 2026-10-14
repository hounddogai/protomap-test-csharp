namespace Acme.Workforce.Orgdesign.Position.PositionThreshold;

using Acme.Workforce.Kit.Text;

/// <summary>Where a position draft is in its life cycle.</summary>
public enum PositionDraftStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>A position draft. Its code is unique within a tenant.</summary>
public sealed class PositionDraft(string code, string name, string summary, bool pinned, string location, double score, PositionDraftStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Summary { get; set; } = summary;
    public bool Pinned { get; set; } = pinned;
    public string Location { get; set; } = location;
    public double Score { get; set; } = score;
    public PositionDraftStatus Status { get; set; } = status;

    /// <summary>Starts a position draft in status open.</summary>
    public static PositionDraft NewPositionDraft(string code, string name)
    {
        return new PositionDraft(code, name, "", false, "", 0.0, PositionDraftStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PositionDraftStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PositionDraftStatus next)
    {
        switch (Status)
        {
            case PositionDraftStatus.Open:
                return next == PositionDraftStatus.Resolved || next == PositionDraftStatus.Reopened;
            case PositionDraftStatus.InReview:
                return next == PositionDraftStatus.Resolved || next == PositionDraftStatus.Reopened;
            case PositionDraftStatus.Resolved:
                switch (next)
                {
                    case PositionDraftStatus.Archived:
                        return true;
                }
                return false;
            case PositionDraftStatus.Reopened:
                return next == PositionDraftStatus.Archived;
        }
        return false;
    }

    public void MoveTo(PositionDraftStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"position draft {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} ({Name})";
    }

    public double MeasuredValue()
    {
        return Score;
    }

    public PositionDraft Copy()
    {
        return new PositionDraft(Code, Name, Summary, Pinned, Location, Score, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
