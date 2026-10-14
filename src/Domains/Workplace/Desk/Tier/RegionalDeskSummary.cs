namespace Acme.Workforce.Workplace.Desk.Tier;

using Acme.Workforce.Kit.Text;

/// <summary>Where a regional desk summary is in its life cycle.</summary>
public enum RegionalDeskSummaryStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>A regional desk summary as the desk team keeps it.</summary>
public sealed class RegionalDeskSummary(string code, string title, string phone, bool active, int windowDays, string timezone, List<string> tags, RegionalDeskSummaryStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Phone { get; set; } = phone;
    public bool Active { get; set; } = active;
    public int WindowDays { get; set; } = windowDays;
    public string Timezone { get; set; } = timezone;
    public List<string> Tags { get; set; } = tags;
    public RegionalDeskSummaryStatus Status { get; set; } = status;

    /// <summary>Starts a regional desk summary in status proposed.</summary>
    public static RegionalDeskSummary NewRegionalDeskSummary(string code, string title)
    {
        return new RegionalDeskSummary(
            code,
            title,
            "",
            false,
            0,
            "",
            new List<string>(),
            RegionalDeskSummaryStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RegionalDeskSummaryStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RegionalDeskSummaryStatus next)
    {
        switch (Status)
        {
            case RegionalDeskSummaryStatus.Proposed:
                return next == RegionalDeskSummaryStatus.Approved || next == RegionalDeskSummaryStatus.Rejected;
            case RegionalDeskSummaryStatus.Approved:
                switch (next)
                {
                    case RegionalDeskSummaryStatus.Rejected:
                    case RegionalDeskSummaryStatus.Withdrawn:
                        return true;
                }
                return false;
            case RegionalDeskSummaryStatus.Rejected:
                switch (next)
                {
                    case RegionalDeskSummaryStatus.Withdrawn:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(RegionalDeskSummaryStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"regional desk summary {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"regional desk summary"} {Code}";
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

    public double MeasuredValue()
    {
        return (double)WindowDays;
    }

    public RegionalDeskSummary Copy()
    {
        return new RegionalDeskSummary(Code, Title, Phone, Active, WindowDays, Timezone, Tags, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
