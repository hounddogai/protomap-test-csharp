namespace Acme.Workforce.Contractors.StatementOfWork.Summary;

using Acme.Workforce.Kit.Text;

/// <summary>Where a statement of work summary is in its life cycle.</summary>
public enum StatementOfWorkSummaryStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>A statement of work summary as the statement of work team keeps it.</summary>
public sealed class StatementOfWorkSummary(string code, string label, string notes, string region, bool visible, int quantity, StatementOfWorkSummaryStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Notes { get; set; } = notes;
    public string Region { get; set; } = region;
    public bool Visible { get; set; } = visible;
    public int Quantity { get; set; } = quantity;
    public StatementOfWorkSummaryStatus Status { get; set; } = status;

    /// <summary>Starts a statement of work summary in status idle.</summary>
    public static StatementOfWorkSummary NewStatementOfWorkSummary(string code, string label)
    {
        return new StatementOfWorkSummary(code, label, "", "", false, 0, StatementOfWorkSummaryStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case StatementOfWorkSummaryStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(StatementOfWorkSummaryStatus next)
    {
        switch (Status)
        {
            case StatementOfWorkSummaryStatus.Idle:
                return next == StatementOfWorkSummaryStatus.Draining;
            case StatementOfWorkSummaryStatus.Warming:
                return next == StatementOfWorkSummaryStatus.Ready || next == StatementOfWorkSummaryStatus.Draining;
            case StatementOfWorkSummaryStatus.Ready:
                return next == StatementOfWorkSummaryStatus.Draining;
            case StatementOfWorkSummaryStatus.Draining:
                switch (next)
                {
                    case StatementOfWorkSummaryStatus.Stopped:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(StatementOfWorkSummaryStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"statement of work summary {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} ({Label})";
    }

    public double MeasuredValue()
    {
        return (double)Quantity;
    }

    public StatementOfWorkSummary Copy()
    {
        return new StatementOfWorkSummary(Code, Label, Notes, Region, Visible, Quantity, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Region);
    }
}
