namespace Acme.Workforce.Compensation.BonusPool.Claim;

using Acme.Workforce.Kit.Text;

/// <summary>Where a lenient bonus pool draft is in its life cycle.</summary>
public enum LenientBonusPoolDraftStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>The record of a lenient bonus pool draft and its current status.</summary>
public sealed class LenientBonusPoolDraft(string code, string title, string salaryBand, int attempts, double minimum, double hours, LenientBonusPoolDraftStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string SalaryBand { get; set; } = salaryBand;
    public int Attempts { get; set; } = attempts;
    public double Minimum { get; set; } = minimum;
    public double Hours { get; set; } = hours;
    public LenientBonusPoolDraftStatus Status { get; set; } = status;

    /// <summary>Starts a lenient bonus pool draft in status idle.</summary>
    public static LenientBonusPoolDraft NewLenientBonusPoolDraft(string code, string title)
    {
        return new LenientBonusPoolDraft(code, title, "", 0, 0.0, 0.0, LenientBonusPoolDraftStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LenientBonusPoolDraftStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LenientBonusPoolDraftStatus next)
    {
        switch (Status)
        {
            case LenientBonusPoolDraftStatus.Idle:
                switch (next)
                {
                    case LenientBonusPoolDraftStatus.Ready:
                        return true;
                }
                return false;
            case LenientBonusPoolDraftStatus.Warming:
                return next == LenientBonusPoolDraftStatus.Draining;
            case LenientBonusPoolDraftStatus.Ready:
                switch (next)
                {
                    case LenientBonusPoolDraftStatus.Draining:
                    case LenientBonusPoolDraftStatus.Stopped:
                        return true;
                }
                return false;
            case LenientBonusPoolDraftStatus.Draining:
                switch (next)
                {
                    case LenientBonusPoolDraftStatus.Stopped:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(LenientBonusPoolDraftStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"lenient bonus pool draft {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"lenient bonus pool draft"} {Code}";
    }

    public double ProjectedLoad()
    {
        return Math.Round((double)Attempts + Minimum, 2);
    }

    public LenientBonusPoolDraft Copy()
    {
        return new LenientBonusPoolDraft(Code, Title, SalaryBand, Attempts, Minimum, Hours, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
