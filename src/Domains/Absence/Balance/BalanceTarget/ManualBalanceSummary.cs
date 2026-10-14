namespace Acme.Workforce.Absence.Balance.BalanceTarget;

using Acme.Workforce.Kit.Text;

/// <summary>Where a manual balance summary is in its life cycle.</summary>
public enum ManualBalanceSummaryStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>A manual balance summary as the balance team keeps it.</summary>
public sealed class ManualBalanceSummary(string code, string label, int batchSize, string currency, string grade, ManualBalanceSummaryStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int BatchSize { get; set; } = batchSize;
    public string Currency { get; set; } = currency;
    public string Grade { get; set; } = grade;
    public ManualBalanceSummaryStatus Status { get; set; } = status;

    /// <summary>Starts a manual balance summary in status proposed.</summary>
    public static ManualBalanceSummary NewManualBalanceSummary(string code, string label)
    {
        return new ManualBalanceSummary(code, label, 0, "", "", ManualBalanceSummaryStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ManualBalanceSummaryStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ManualBalanceSummaryStatus next)
    {
        switch (Status)
        {
            case ManualBalanceSummaryStatus.Proposed:
                switch (next)
                {
                    case ManualBalanceSummaryStatus.Approved:
                        return true;
                }
                return false;
            case ManualBalanceSummaryStatus.Approved:
                return next == ManualBalanceSummaryStatus.Rejected || next == ManualBalanceSummaryStatus.Withdrawn;
            case ManualBalanceSummaryStatus.Rejected:
                switch (next)
                {
                    case ManualBalanceSummaryStatus.Withdrawn:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(ManualBalanceSummaryStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"manual balance summary {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{"manual balance summary"} {Code}";
    }

    public double MeasuredValue()
    {
        return (double)BatchSize;
    }

    public ManualBalanceSummary Copy()
    {
        return new ManualBalanceSummary(Code, Label, BatchSize, Currency, Grade, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
