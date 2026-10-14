namespace Acme.Workforce.Absence.Accrual.Threshold;

using Acme.Workforce.Kit.Text;

/// <summary>Where a accrual summary is in its life cycle.</summary>
public enum AccrualSummaryStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>The record of a accrual summary and its current status.</summary>
public sealed class AccrualSummary(string code, string name, bool visible, string contactEmail, string skill, string reviewer, string currency, string summary, int quantity, AccrualSummaryStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public bool Visible { get; set; } = visible;
    public string ContactEmail { get; set; } = contactEmail;
    public string Skill { get; set; } = skill;
    public string Reviewer { get; set; } = reviewer;
    public string Currency { get; set; } = currency;
    public string Summary { get; set; } = summary;
    public int Quantity { get; set; } = quantity;
    public AccrualSummaryStatus Status { get; set; } = status;

    /// <summary>Starts a accrual summary in status draft.</summary>
    public static AccrualSummary NewAccrualSummary(string code, string name)
    {
        return new AccrualSummary(code, name, false, "", "", "", "", "", 0, AccrualSummaryStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AccrualSummaryStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AccrualSummaryStatus next)
    {
        switch (Status)
        {
            case AccrualSummaryStatus.Draft:
                return next == AccrualSummaryStatus.Active || next == AccrualSummaryStatus.Suspended;
            case AccrualSummaryStatus.Pending:
                switch (next)
                {
                    case AccrualSummaryStatus.Closed:
                        return true;
                }
                return false;
            case AccrualSummaryStatus.Active:
                switch (next)
                {
                    case AccrualSummaryStatus.Suspended:
                    case AccrualSummaryStatus.Closed:
                        return true;
                }
                return false;
            case AccrualSummaryStatus.Suspended:
                return next == AccrualSummaryStatus.Closed;
        }
        return false;
    }

    public void MoveTo(AccrualSummaryStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"accrual summary {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"accrual summary"} {Code}";
    }

    public double MeasuredValue()
    {
        return (double)Quantity;
    }

    public AccrualSummary Copy()
    {
        return new AccrualSummary(
            Code,
            Name,
            Visible,
            ContactEmail,
            Skill,
            Reviewer,
            Currency,
            Summary,
            Quantity,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
