namespace Acme.Workforce.Workplace.Visitor.Summary;

using Acme.Workforce.Kit.Text;

/// <summary>Where a manual visitor summary is in its life cycle.</summary>
public enum ManualVisitorSummaryStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>One manual visitor summary, with what the planners need to act on it.</summary>
public sealed class ManualVisitorSummary(string code, string name, int retries, bool archived, string managerId, string label, double floorValue, string channel, string reviewer, ManualVisitorSummaryStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int Retries { get; set; } = retries;
    public bool Archived { get; set; } = archived;
    public string ManagerId { get; set; } = managerId;
    public string Label { get; set; } = label;
    public double FloorValue { get; set; } = floorValue;
    public string Channel { get; set; } = channel;
    public string Reviewer { get; set; } = reviewer;
    public ManualVisitorSummaryStatus Status { get; set; } = status;

    /// <summary>Starts a manual visitor summary in status open.</summary>
    public static ManualVisitorSummary NewManualVisitorSummary(string code, string name)
    {
        return new ManualVisitorSummary(code, name, 0, false, "", "", 0.0, "", "", ManualVisitorSummaryStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ManualVisitorSummaryStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ManualVisitorSummaryStatus next)
    {
        switch (Status)
        {
            case ManualVisitorSummaryStatus.Open:
                return next == ManualVisitorSummaryStatus.InReview || next == ManualVisitorSummaryStatus.Resolved;
            case ManualVisitorSummaryStatus.InReview:
                switch (next)
                {
                    case ManualVisitorSummaryStatus.Reopened:
                    case ManualVisitorSummaryStatus.Archived:
                        return true;
                }
                return false;
            case ManualVisitorSummaryStatus.Resolved:
                switch (next)
                {
                    case ManualVisitorSummaryStatus.Reopened:
                        return true;
                }
                return false;
            case ManualVisitorSummaryStatus.Reopened:
                switch (next)
                {
                    case ManualVisitorSummaryStatus.Archived:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(ManualVisitorSummaryStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"manual visitor summary {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{"manual visitor summary"} {Code}";
    }

    public double NetLoad()
    {
        return Math.Round(FloorValue + (double)Retries, 2);
    }

    public ManualVisitorSummary Copy()
    {
        return new ManualVisitorSummary(
            Code,
            Name,
            Retries,
            Archived,
            ManagerId,
            Label,
            FloorValue,
            Channel,
            Reviewer,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Channel);
    }
}
