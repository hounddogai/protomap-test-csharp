namespace Acme.Workforce.Orgdesign.Headcount.HeadcountRecord;

using Acme.Workforce.Kit.Text;

/// <summary>Where a default headcount draft is in its life cycle.</summary>
public enum DefaultHeadcountDraftStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>A default headcount draft. Its code is unique within a tenant.</summary>
public sealed class DefaultHeadcountDraft(string code, string label, int startsAt, double cost, string title, DefaultHeadcountDraftStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int StartsAt { get; set; } = startsAt;
    public double Cost { get; set; } = cost;
    public string Title { get; set; } = title;
    public DefaultHeadcountDraftStatus Status { get; set; } = status;

    /// <summary>Starts a default headcount draft in status new.</summary>
    public static DefaultHeadcountDraft NewDefaultHeadcountDraft(string code, string label)
    {
        return new DefaultHeadcountDraft(code, label, 0, 0.0, "", DefaultHeadcountDraftStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case DefaultHeadcountDraftStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(DefaultHeadcountDraftStatus next)
    {
        switch (Status)
        {
            case DefaultHeadcountDraftStatus.New:
                switch (next)
                {
                    case DefaultHeadcountDraftStatus.Running:
                    case DefaultHeadcountDraftStatus.Failed:
                        return true;
                }
                return false;
            case DefaultHeadcountDraftStatus.Queued:
                return next == DefaultHeadcountDraftStatus.Done;
            case DefaultHeadcountDraftStatus.Running:
                return next == DefaultHeadcountDraftStatus.Done;
            case DefaultHeadcountDraftStatus.Done:
                return next == DefaultHeadcountDraftStatus.Failed;
        }
        return false;
    }

    public void MoveTo(DefaultHeadcountDraftStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"default headcount draft {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        StartsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{Code} ({Label})";
    }

    public double WeightedLoad()
    {
        return Math.Round(Cost - (double)StartsAt, 2);
    }

    public DefaultHeadcountDraft Copy()
    {
        return new DefaultHeadcountDraft(Code, Label, StartsAt, Cost, Title, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
