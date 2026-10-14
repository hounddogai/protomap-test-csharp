namespace Acme.Workforce.Workplace.Desk.Digest;

using Acme.Workforce.Kit.Text;

/// <summary>Where a desk summary is in its life cycle.</summary>
public enum DeskSummaryStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>One desk summary, with what the planners need to act on it.</summary>
public sealed class DeskSummary(string code, string label, int leaveDays, string category, string phone, int priority, DeskSummaryStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int LeaveDays { get; set; } = leaveDays;
    public string Category { get; set; } = category;
    public string Phone { get; set; } = phone;
    public int Priority { get; set; } = priority;
    public DeskSummaryStatus Status { get; set; } = status;

    /// <summary>Starts a desk summary in status idle.</summary>
    public static DeskSummary NewDeskSummary(string code, string label)
    {
        return new DeskSummary(code, label, 0, "", "", 0, DeskSummaryStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case DeskSummaryStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(DeskSummaryStatus next)
    {
        switch (Status)
        {
            case DeskSummaryStatus.Idle:
                switch (next)
                {
                    case DeskSummaryStatus.Ready:
                        return true;
                }
                return false;
            case DeskSummaryStatus.Warming:
                switch (next)
                {
                    case DeskSummaryStatus.Ready:
                    case DeskSummaryStatus.Stopped:
                        return true;
                }
                return false;
            case DeskSummaryStatus.Ready:
                return next == DeskSummaryStatus.Draining;
            case DeskSummaryStatus.Draining:
                return next == DeskSummaryStatus.Stopped;
        }
        return false;
    }

    public void MoveTo(DeskSummaryStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"desk summary {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{"desk summary"} {Code}";
    }

    public double ProjectedVolume()
    {
        return Math.Round((double)Priority * (double)LeaveDays, 2);
    }

    public DeskSummary Copy()
    {
        return new DeskSummary(Code, Label, LeaveDays, Category, Phone, Priority, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Category);
    }
}
