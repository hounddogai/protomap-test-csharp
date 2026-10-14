namespace Acme.Workforce.Compensation.Benchmark.Rollup;

using Acme.Workforce.Kit.Text;

/// <summary>Where a manual benchmark window is in its life cycle.</summary>
public enum ManualBenchmarkWindowStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>The record of a manual benchmark window and its current status.</summary>
public sealed class ManualBenchmarkWindow(string code, string title, string managerId, bool visible, string reviewer, bool pinned, string label, int leaveDays, ManualBenchmarkWindowStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string ManagerId { get; set; } = managerId;
    public bool Visible { get; set; } = visible;
    public string Reviewer { get; set; } = reviewer;
    public bool Pinned { get; set; } = pinned;
    public string Label { get; set; } = label;
    public int LeaveDays { get; set; } = leaveDays;
    public ManualBenchmarkWindowStatus Status { get; set; } = status;

    /// <summary>Starts a manual benchmark window in status draft.</summary>
    public static ManualBenchmarkWindow NewManualBenchmarkWindow(string code, string title)
    {
        return new ManualBenchmarkWindow(
            code,
            title,
            "",
            false,
            "",
            false,
            "",
            0,
            ManualBenchmarkWindowStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ManualBenchmarkWindowStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ManualBenchmarkWindowStatus next)
    {
        switch (Status)
        {
            case ManualBenchmarkWindowStatus.Draft:
                return next == ManualBenchmarkWindowStatus.Pending;
            case ManualBenchmarkWindowStatus.Pending:
                return next == ManualBenchmarkWindowStatus.Closed;
            case ManualBenchmarkWindowStatus.Active:
                switch (next)
                {
                    case ManualBenchmarkWindowStatus.Suspended:
                    case ManualBenchmarkWindowStatus.Closed:
                        return true;
                }
                return false;
            case ManualBenchmarkWindowStatus.Suspended:
                return next == ManualBenchmarkWindowStatus.Closed;
        }
        return false;
    }

    public void MoveTo(ManualBenchmarkWindowStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"manual benchmark window {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} ({Title})";
    }

    public double MeasuredValue()
    {
        return (double)LeaveDays;
    }

    public ManualBenchmarkWindow Copy()
    {
        return new ManualBenchmarkWindow(
            Code,
            Title,
            ManagerId,
            Visible,
            Reviewer,
            Pinned,
            Label,
            LeaveDays,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
