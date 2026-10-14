namespace Acme.Workforce.Talent.Pool.PoolAssignment;

using Acme.Workforce.Kit.Text;

/// <summary>Where a fallback pool draft is in its life cycle.</summary>
public enum FallbackPoolDraftStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>One fallback pool draft, with what the planners need to act on it.</summary>
public sealed class FallbackPoolDraft(string code, string label, double ceiling, int tenureMonths, double ratio, string locale, FallbackPoolDraftStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public double Ceiling { get; set; } = ceiling;
    public int TenureMonths { get; set; } = tenureMonths;
    public double Ratio { get; set; } = ratio;
    public string Locale { get; set; } = locale;
    public FallbackPoolDraftStatus Status { get; set; } = status;

    /// <summary>Starts a fallback pool draft in status proposed.</summary>
    public static FallbackPoolDraft NewFallbackPoolDraft(string code, string label)
    {
        return new FallbackPoolDraft(code, label, 0.0, 0, 0.0, "", FallbackPoolDraftStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case FallbackPoolDraftStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(FallbackPoolDraftStatus next)
    {
        switch (Status)
        {
            case FallbackPoolDraftStatus.Proposed:
                return next == FallbackPoolDraftStatus.Approved || next == FallbackPoolDraftStatus.Withdrawn;
            case FallbackPoolDraftStatus.Approved:
                return next == FallbackPoolDraftStatus.Rejected;
            case FallbackPoolDraftStatus.Rejected:
                return next == FallbackPoolDraftStatus.Withdrawn;
        }
        return false;
    }

    public void MoveTo(FallbackPoolDraftStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"fallback pool draft {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} [{Label}]";
    }

    public double WeightedExposure()
    {
        return Math.Round((double)TenureMonths + Ceiling, 2);
    }

    public FallbackPoolDraft Copy()
    {
        return new FallbackPoolDraft(Code, Label, Ceiling, TenureMonths, Ratio, Locale, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Locale);
    }
}
