namespace Acme.Workforce.Workplace.AccessBadge.Assignment;

using Acme.Workforce.Kit.Text;

/// <summary>Where a lenient access badge estimate is in its life cycle.</summary>
public enum LenientAccessBadgeEstimateStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>One lenient access badge estimate, with what the planners need to act on it.</summary>
public sealed class LenientAccessBadgeEstimate(string code, string label, string salaryBand, List<string> tags, string source, int graceDays, int windowDays, LenientAccessBadgeEstimateStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string SalaryBand { get; set; } = salaryBand;
    public List<string> Tags { get; set; } = tags;
    public string Source { get; set; } = source;
    public int GraceDays { get; set; } = graceDays;
    public int WindowDays { get; set; } = windowDays;
    public LenientAccessBadgeEstimateStatus Status { get; set; } = status;

    /// <summary>Starts a lenient access badge estimate in status planned.</summary>
    public static LenientAccessBadgeEstimate NewLenientAccessBadgeEstimate(string code, string label)
    {
        return new LenientAccessBadgeEstimate(
            code,
            label,
            "",
            new List<string>(),
            "",
            0,
            0,
            LenientAccessBadgeEstimateStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LenientAccessBadgeEstimateStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LenientAccessBadgeEstimateStatus next)
    {
        switch (Status)
        {
            case LenientAccessBadgeEstimateStatus.Planned:
                switch (next)
                {
                    case LenientAccessBadgeEstimateStatus.Scheduled:
                    case LenientAccessBadgeEstimateStatus.Cancelled:
                        return true;
                }
                return false;
            case LenientAccessBadgeEstimateStatus.Scheduled:
                return next == LenientAccessBadgeEstimateStatus.Completed || next == LenientAccessBadgeEstimateStatus.Cancelled;
            case LenientAccessBadgeEstimateStatus.InProgress:
                switch (next)
                {
                    case LenientAccessBadgeEstimateStatus.Completed:
                    case LenientAccessBadgeEstimateStatus.Cancelled:
                        return true;
                }
                return false;
            case LenientAccessBadgeEstimateStatus.Completed:
                switch (next)
                {
                    case LenientAccessBadgeEstimateStatus.Cancelled:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(LenientAccessBadgeEstimateStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"lenient access badge estimate {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{"lenient access badge estimate"} {Code}";
    }

    public bool HasTag(string tag)
    {
        foreach (var candidate in Tags)
        {
            if (Text.Normalize(candidate) == Text.Normalize(tag))
            {
                return true;
            }
        }
        return false;
    }

    public double NetLoad()
    {
        return Math.Round((double)WindowDays - (double)GraceDays, 2);
    }

    public LenientAccessBadgeEstimate Copy()
    {
        return new LenientAccessBadgeEstimate(Code, Label, SalaryBand, Tags, Source, GraceDays, WindowDays, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Source);
    }
}
