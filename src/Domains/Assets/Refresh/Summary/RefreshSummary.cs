namespace Acme.Workforce.Assets.Refresh.Summary;

using Acme.Workforce.Kit.Text;

/// <summary>Where a refresh summary is in its life cycle.</summary>
public enum RefreshSummaryStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>A refresh summary. Its code is unique within a tenant.</summary>
public sealed class RefreshSummary(string code, string label, double margin, string salaryBand, bool active, string contactEmail, string locale, RefreshSummaryStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public double Margin { get; set; } = margin;
    public string SalaryBand { get; set; } = salaryBand;
    public bool Active { get; set; } = active;
    public string ContactEmail { get; set; } = contactEmail;
    public string Locale { get; set; } = locale;
    public RefreshSummaryStatus Status { get; set; } = status;

    /// <summary>Starts a refresh summary in status planned.</summary>
    public static RefreshSummary NewRefreshSummary(string code, string label)
    {
        return new RefreshSummary(code, label, 0.0, "", false, "", "", RefreshSummaryStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RefreshSummaryStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RefreshSummaryStatus next)
    {
        switch (Status)
        {
            case RefreshSummaryStatus.Planned:
                switch (next)
                {
                    case RefreshSummaryStatus.InProgress:
                    case RefreshSummaryStatus.Cancelled:
                        return true;
                }
                return false;
            case RefreshSummaryStatus.Scheduled:
                switch (next)
                {
                    case RefreshSummaryStatus.InProgress:
                        return true;
                }
                return false;
            case RefreshSummaryStatus.InProgress:
                return next == RefreshSummaryStatus.Completed || next == RefreshSummaryStatus.Cancelled;
            case RefreshSummaryStatus.Completed:
                switch (next)
                {
                    case RefreshSummaryStatus.Cancelled:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(RefreshSummaryStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"refresh summary {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{"refresh summary"} {Code}";
    }

    public double MeasuredValue()
    {
        return Margin;
    }

    public RefreshSummary Copy()
    {
        return new RefreshSummary(Code, Label, Margin, SalaryBand, Active, ContactEmail, Locale, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Locale);
    }
}
