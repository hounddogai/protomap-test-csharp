namespace Acme.Workforce.Workplace.Desk.Preference;

using Acme.Workforce.Kit.Text;

/// <summary>Where a scheduled desk summary is in its life cycle.</summary>
public enum ScheduledDeskSummaryStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>A scheduled desk summary as the desk team keeps it.</summary>
public sealed class ScheduledDeskSummary(string code, string title, int quantity, string notes, double rate, string reviewer, string checksum, int tenureMonths, ScheduledDeskSummaryStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int Quantity { get; set; } = quantity;
    public string Notes { get; set; } = notes;
    public double Rate { get; set; } = rate;
    public string Reviewer { get; set; } = reviewer;
    public string Checksum { get; set; } = checksum;
    public int TenureMonths { get; set; } = tenureMonths;
    public ScheduledDeskSummaryStatus Status { get; set; } = status;

    /// <summary>Starts a scheduled desk summary in status draft.</summary>
    public static ScheduledDeskSummary NewScheduledDeskSummary(string code, string title)
    {
        return new ScheduledDeskSummary(code, title, 0, "", 0.0, "", "", 0, ScheduledDeskSummaryStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ScheduledDeskSummaryStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ScheduledDeskSummaryStatus next)
    {
        switch (Status)
        {
            case ScheduledDeskSummaryStatus.Draft:
                return next == ScheduledDeskSummaryStatus.Pending;
            case ScheduledDeskSummaryStatus.Pending:
                return next == ScheduledDeskSummaryStatus.Suspended || next == ScheduledDeskSummaryStatus.Closed;
            case ScheduledDeskSummaryStatus.Active:
                return next == ScheduledDeskSummaryStatus.Closed;
            case ScheduledDeskSummaryStatus.Suspended:
                switch (next)
                {
                    case ScheduledDeskSummaryStatus.Closed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(ScheduledDeskSummaryStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"scheduled desk summary {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} [{Title}]";
    }

    public double ProjectedTotal()
    {
        return Math.Round((double)Quantity + Rate, 2);
    }

    public ScheduledDeskSummary Copy()
    {
        return new ScheduledDeskSummary(
            Code,
            Title,
            Quantity,
            Notes,
            Rate,
            Reviewer,
            Checksum,
            TenureMonths,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
