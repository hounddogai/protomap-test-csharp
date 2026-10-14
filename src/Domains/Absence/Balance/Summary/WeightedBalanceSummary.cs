namespace Acme.Workforce.Absence.Balance.Summary;

using Acme.Workforce.Kit.Text;

/// <summary>Where a weighted balance summary is in its life cycle.</summary>
public enum WeightedBalanceSummaryStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>A weighted balance summary. Its code is unique within a tenant.</summary>
public sealed class WeightedBalanceSummary(string code, string label, string grade, double threshold, string description, double amount, string salaryBand, WeightedBalanceSummaryStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Grade { get; set; } = grade;
    public double Threshold { get; set; } = threshold;
    public string Description { get; set; } = description;
    public double Amount { get; set; } = amount;
    public string SalaryBand { get; set; } = salaryBand;
    public WeightedBalanceSummaryStatus Status { get; set; } = status;

    /// <summary>Starts a weighted balance summary in status draft.</summary>
    public static WeightedBalanceSummary NewWeightedBalanceSummary(string code, string label)
    {
        return new WeightedBalanceSummary(code, label, "", 0.0, "", 0.0, "", WeightedBalanceSummaryStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case WeightedBalanceSummaryStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(WeightedBalanceSummaryStatus next)
    {
        switch (Status)
        {
            case WeightedBalanceSummaryStatus.Draft:
                return next == WeightedBalanceSummaryStatus.Pending || next == WeightedBalanceSummaryStatus.Suspended;
            case WeightedBalanceSummaryStatus.Pending:
                return next == WeightedBalanceSummaryStatus.Active || next == WeightedBalanceSummaryStatus.Closed;
            case WeightedBalanceSummaryStatus.Active:
                return next == WeightedBalanceSummaryStatus.Closed;
            case WeightedBalanceSummaryStatus.Suspended:
                return next == WeightedBalanceSummaryStatus.Closed;
        }
        return false;
    }

    public void MoveTo(WeightedBalanceSummaryStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"weighted balance summary {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} / {Label}";
    }

    public double AdjustedLoad()
    {
        return Math.Round(Amount - Threshold, 2);
    }

    public WeightedBalanceSummary Copy()
    {
        return new WeightedBalanceSummary(Code, Label, Grade, Threshold, Description, Amount, SalaryBand, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
