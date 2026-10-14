namespace Acme.Workforce.Absence.Balance.Baseline;

using Acme.Workforce.Kit.Text;

/// <summary>Where a adaptive balance baseline is in its life cycle.</summary>
public enum AdaptiveBalanceBaselineStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>The record of a adaptive balance baseline and its current status.</summary>
public sealed class AdaptiveBalanceBaseline(string code, string title, string skill, int windowDays, string source, AdaptiveBalanceBaselineStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Skill { get; set; } = skill;
    public int WindowDays { get; set; } = windowDays;
    public string Source { get; set; } = source;
    public AdaptiveBalanceBaselineStatus Status { get; set; } = status;

    /// <summary>Starts a adaptive balance baseline in status proposed.</summary>
    public static AdaptiveBalanceBaseline NewAdaptiveBalanceBaseline(string code, string title)
    {
        return new AdaptiveBalanceBaseline(code, title, "", 0, "", AdaptiveBalanceBaselineStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AdaptiveBalanceBaselineStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AdaptiveBalanceBaselineStatus next)
    {
        switch (Status)
        {
            case AdaptiveBalanceBaselineStatus.Proposed:
                switch (next)
                {
                    case AdaptiveBalanceBaselineStatus.Rejected:
                        return true;
                }
                return false;
            case AdaptiveBalanceBaselineStatus.Approved:
                return next == AdaptiveBalanceBaselineStatus.Rejected || next == AdaptiveBalanceBaselineStatus.Withdrawn;
            case AdaptiveBalanceBaselineStatus.Rejected:
                switch (next)
                {
                    case AdaptiveBalanceBaselineStatus.Withdrawn:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(AdaptiveBalanceBaselineStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"adaptive balance baseline {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} [{Title}]";
    }

    public double MeasuredValue()
    {
        return (double)WindowDays;
    }

    public AdaptiveBalanceBaseline Copy()
    {
        return new AdaptiveBalanceBaseline(Code, Title, Skill, WindowDays, Source, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Source);
    }
}
