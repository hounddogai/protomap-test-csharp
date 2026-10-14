namespace Acme.Workforce.Orgdesign.Headcount.Draft;

using Acme.Workforce.Kit.Text;

/// <summary>Where a primary headcount draft is in its life cycle.</summary>
public enum PrimaryHeadcountDraftStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>A primary headcount draft as the headcount team keeps it.</summary>
public sealed class PrimaryHeadcountDraft(string code, string title, Dictionary<string, string> attributes, double rate, bool approved, PrimaryHeadcountDraftStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public double Rate { get; set; } = rate;
    public bool Approved { get; set; } = approved;
    public PrimaryHeadcountDraftStatus Status { get; set; } = status;

    /// <summary>Starts a primary headcount draft in status open.</summary>
    public static PrimaryHeadcountDraft NewPrimaryHeadcountDraft(string code, string title)
    {
        return new PrimaryHeadcountDraft(
            code,
            title,
            new Dictionary<string, string>(),
            0.0,
            false,
            PrimaryHeadcountDraftStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PrimaryHeadcountDraftStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PrimaryHeadcountDraftStatus next)
    {
        switch (Status)
        {
            case PrimaryHeadcountDraftStatus.Open:
                return next == PrimaryHeadcountDraftStatus.InReview || next == PrimaryHeadcountDraftStatus.Reopened;
            case PrimaryHeadcountDraftStatus.InReview:
                return next == PrimaryHeadcountDraftStatus.Reopened;
            case PrimaryHeadcountDraftStatus.Resolved:
                return next == PrimaryHeadcountDraftStatus.Reopened || next == PrimaryHeadcountDraftStatus.Archived;
            case PrimaryHeadcountDraftStatus.Reopened:
                switch (next)
                {
                    case PrimaryHeadcountDraftStatus.Archived:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(PrimaryHeadcountDraftStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"primary headcount draft {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"primary headcount draft"} {Code}";
    }

    public string Attribute(string key, string fallback)
    {
        if (!Attributes.TryGetValue(key, out var found))
        {
            return fallback;
        }
        if (found.Trim() == "")
        {
            return fallback;
        }
        return found;
    }

    public double MeasuredValue()
    {
        return Rate;
    }

    public PrimaryHeadcountDraft Copy()
    {
        return new PrimaryHeadcountDraft(Code, Title, Attributes, Rate, Approved, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
