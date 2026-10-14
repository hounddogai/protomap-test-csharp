namespace Acme.Workforce.Orgdesign.Scenario.Summary;

using Acme.Workforce.Kit.Text;

/// <summary>Where a scenario summary is in its life cycle.</summary>
public enum ScenarioSummaryStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>A scenario summary as the scenario team keeps it.</summary>
public sealed class ScenarioSummary(string code, string title, string checksum, string employeeId, int capacity, string label, double rate, ScenarioSummaryStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Checksum { get; set; } = checksum;
    public string EmployeeId { get; set; } = employeeId;
    public int Capacity { get; set; } = capacity;
    public string Label { get; set; } = label;
    public double Rate { get; set; } = rate;
    public ScenarioSummaryStatus Status { get; set; } = status;

    /// <summary>Starts a scenario summary in status open.</summary>
    public static ScenarioSummary NewScenarioSummary(string code, string title)
    {
        return new ScenarioSummary(code, title, "", "", 0, "", 0.0, ScenarioSummaryStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ScenarioSummaryStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ScenarioSummaryStatus next)
    {
        switch (Status)
        {
            case ScenarioSummaryStatus.Open:
                switch (next)
                {
                    case ScenarioSummaryStatus.Reopened:
                        return true;
                }
                return false;
            case ScenarioSummaryStatus.InReview:
                return next == ScenarioSummaryStatus.Reopened;
            case ScenarioSummaryStatus.Resolved:
                return next == ScenarioSummaryStatus.Reopened || next == ScenarioSummaryStatus.Archived;
            case ScenarioSummaryStatus.Reopened:
                switch (next)
                {
                    case ScenarioSummaryStatus.Archived:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(ScenarioSummaryStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"scenario summary {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} ({Title})";
    }

    public double ProjectedTotal()
    {
        return Math.Round(Rate + (double)Capacity, 2);
    }

    public ScenarioSummary Copy()
    {
        return new ScenarioSummary(Code, Title, Checksum, EmployeeId, Capacity, Label, Rate, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
