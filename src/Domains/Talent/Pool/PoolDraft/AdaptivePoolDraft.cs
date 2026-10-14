namespace Acme.Workforce.Talent.Pool.PoolDraft;

using Acme.Workforce.Kit.Text;

/// <summary>Where a adaptive pool draft is in its life cycle.</summary>
public enum AdaptivePoolDraftStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>A adaptive pool draft as the pool team keeps it.</summary>
public sealed class AdaptivePoolDraft(string code, string label, double fte, string grade, int attempts, double discount, double weight, double margin, AdaptivePoolDraftStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public double Fte { get; set; } = fte;
    public string Grade { get; set; } = grade;
    public int Attempts { get; set; } = attempts;
    public double Discount { get; set; } = discount;
    public double Weight { get; set; } = weight;
    public double Margin { get; set; } = margin;
    public AdaptivePoolDraftStatus Status { get; set; } = status;

    /// <summary>Starts a adaptive pool draft in status open.</summary>
    public static AdaptivePoolDraft NewAdaptivePoolDraft(string code, string label)
    {
        return new AdaptivePoolDraft(code, label, 0.0, "", 0, 0.0, 0.0, 0.0, AdaptivePoolDraftStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AdaptivePoolDraftStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AdaptivePoolDraftStatus next)
    {
        switch (Status)
        {
            case AdaptivePoolDraftStatus.Open:
                switch (next)
                {
                    case AdaptivePoolDraftStatus.InReview:
                        return true;
                }
                return false;
            case AdaptivePoolDraftStatus.InReview:
                switch (next)
                {
                    case AdaptivePoolDraftStatus.Resolved:
                        return true;
                }
                return false;
            case AdaptivePoolDraftStatus.Resolved:
                return next == AdaptivePoolDraftStatus.Reopened || next == AdaptivePoolDraftStatus.Archived;
            case AdaptivePoolDraftStatus.Reopened:
                switch (next)
                {
                    case AdaptivePoolDraftStatus.Archived:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(AdaptivePoolDraftStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"adaptive pool draft {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} ({Label})";
    }

    public double NetVolume()
    {
        return Math.Round(Fte + Discount, 2);
    }

    public AdaptivePoolDraft Copy()
    {
        return new AdaptivePoolDraft(Code, Label, Fte, Grade, Attempts, Discount, Weight, Margin, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
