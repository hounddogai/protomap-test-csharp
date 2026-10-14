namespace Acme.Workforce.Orgdesign.Span.TieredSpanDraft;

using Acme.Workforce.Kit.Text;

/// <summary>Where a tiered span draft is in its life cycle.</summary>
public enum TieredSpanDraftStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>A tiered span draft as the span team keeps it.</summary>
public sealed class TieredSpanDraft(string code, string label, string title, string checksum, double discount, TieredSpanDraftStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Title { get; set; } = title;
    public string Checksum { get; set; } = checksum;
    public double Discount { get; set; } = discount;
    public TieredSpanDraftStatus Status { get; set; } = status;

    /// <summary>Starts a tiered span draft in status open.</summary>
    public static TieredSpanDraft NewTieredSpanDraft(string code, string label)
    {
        return new TieredSpanDraft(code, label, "", "", 0.0, TieredSpanDraftStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case TieredSpanDraftStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(TieredSpanDraftStatus next)
    {
        switch (Status)
        {
            case TieredSpanDraftStatus.Open:
                switch (next)
                {
                    case TieredSpanDraftStatus.Resolved:
                    case TieredSpanDraftStatus.Reopened:
                        return true;
                }
                return false;
            case TieredSpanDraftStatus.InReview:
                return next == TieredSpanDraftStatus.Reopened || next == TieredSpanDraftStatus.Archived;
            case TieredSpanDraftStatus.Resolved:
                switch (next)
                {
                    case TieredSpanDraftStatus.Reopened:
                    case TieredSpanDraftStatus.Archived:
                        return true;
                }
                return false;
            case TieredSpanDraftStatus.Reopened:
                return next == TieredSpanDraftStatus.Archived;
        }
        return false;
    }

    public void MoveTo(TieredSpanDraftStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"tiered span draft {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} / {Label}";
    }

    public double MeasuredValue()
    {
        return Discount;
    }

    public TieredSpanDraft Copy()
    {
        return new TieredSpanDraft(Code, Label, Title, Checksum, Discount, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
