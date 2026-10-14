namespace Acme.Workforce.Compliance.Exception.Baseline;

using Acme.Workforce.Kit.Text;

/// <summary>Where a exception baseline is in its life cycle.</summary>
public enum ExceptionBaselineStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>One exception baseline, with what the planners need to act on it.</summary>
public sealed class ExceptionBaseline(string code, string label, int tenureMonths, string externalRef, int retries, ExceptionBaselineStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int TenureMonths { get; set; } = tenureMonths;
    public string ExternalRef { get; set; } = externalRef;
    public int Retries { get; set; } = retries;
    public ExceptionBaselineStatus Status { get; set; } = status;

    /// <summary>Starts a exception baseline in status open.</summary>
    public static ExceptionBaseline NewExceptionBaseline(string code, string label)
    {
        return new ExceptionBaseline(code, label, 0, "", 0, ExceptionBaselineStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ExceptionBaselineStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ExceptionBaselineStatus next)
    {
        switch (Status)
        {
            case ExceptionBaselineStatus.Open:
                switch (next)
                {
                    case ExceptionBaselineStatus.InReview:
                    case ExceptionBaselineStatus.Archived:
                        return true;
                }
                return false;
            case ExceptionBaselineStatus.InReview:
                switch (next)
                {
                    case ExceptionBaselineStatus.Reopened:
                    case ExceptionBaselineStatus.Archived:
                        return true;
                }
                return false;
            case ExceptionBaselineStatus.Resolved:
                return next == ExceptionBaselineStatus.Reopened;
            case ExceptionBaselineStatus.Reopened:
                switch (next)
                {
                    case ExceptionBaselineStatus.Archived:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(ExceptionBaselineStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"exception baseline {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} ({Label})";
    }

    public double AdjustedExposure()
    {
        return Math.Round((double)Retries + (double)TenureMonths, 2);
    }

    public ExceptionBaseline Copy()
    {
        return new ExceptionBaseline(Code, Label, TenureMonths, ExternalRef, Retries, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
