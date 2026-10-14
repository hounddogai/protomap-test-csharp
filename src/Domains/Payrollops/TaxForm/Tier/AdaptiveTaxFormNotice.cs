namespace Acme.Workforce.Payrollops.TaxForm.Tier;

using Acme.Workforce.Kit.Text;

/// <summary>Where a adaptive tax form notice is in its life cycle.</summary>
public enum AdaptiveTaxFormNoticeStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>One adaptive tax form notice, with what the planners need to act on it.</summary>
public sealed class AdaptiveTaxFormNotice(string code, string title, string timezone, double minimum, string employeeId, string label, bool visible, int windowDays, string reviewer, int batchSize, AdaptiveTaxFormNoticeStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Timezone { get; set; } = timezone;
    public double Minimum { get; set; } = minimum;
    public string EmployeeId { get; set; } = employeeId;
    public string Label { get; set; } = label;
    public bool Visible { get; set; } = visible;
    public int WindowDays { get; set; } = windowDays;
    public string Reviewer { get; set; } = reviewer;
    public int BatchSize { get; set; } = batchSize;
    public AdaptiveTaxFormNoticeStatus Status { get; set; } = status;

    /// <summary>Starts a adaptive tax form notice in status new.</summary>
    public static AdaptiveTaxFormNotice NewAdaptiveTaxFormNotice(string code, string title)
    {
        return new AdaptiveTaxFormNotice(
            code,
            title,
            "",
            0.0,
            "",
            "",
            false,
            0,
            "",
            0,
            AdaptiveTaxFormNoticeStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AdaptiveTaxFormNoticeStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AdaptiveTaxFormNoticeStatus next)
    {
        switch (Status)
        {
            case AdaptiveTaxFormNoticeStatus.New:
                switch (next)
                {
                    case AdaptiveTaxFormNoticeStatus.Queued:
                    case AdaptiveTaxFormNoticeStatus.Failed:
                        return true;
                }
                return false;
            case AdaptiveTaxFormNoticeStatus.Queued:
                switch (next)
                {
                    case AdaptiveTaxFormNoticeStatus.Failed:
                        return true;
                }
                return false;
            case AdaptiveTaxFormNoticeStatus.Running:
                switch (next)
                {
                    case AdaptiveTaxFormNoticeStatus.Failed:
                        return true;
                }
                return false;
            case AdaptiveTaxFormNoticeStatus.Done:
                return next == AdaptiveTaxFormNoticeStatus.Failed;
        }
        return false;
    }

    public void MoveTo(AdaptiveTaxFormNoticeStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"adaptive tax form notice {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{"adaptive tax form notice"} {Code}";
    }

    public double AdjustedExposure()
    {
        return Math.Round((double)BatchSize - Minimum, 2);
    }

    public AdaptiveTaxFormNotice Copy()
    {
        return new AdaptiveTaxFormNotice(
            Code,
            Title,
            Timezone,
            Minimum,
            EmployeeId,
            Label,
            Visible,
            WindowDays,
            Reviewer,
            BatchSize,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
