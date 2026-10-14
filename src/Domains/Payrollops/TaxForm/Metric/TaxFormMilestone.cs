namespace Acme.Workforce.Payrollops.TaxForm.Metric;

using Acme.Workforce.Kit.Text;

/// <summary>Where a tax form milestone is in its life cycle.</summary>
public enum TaxFormMilestoneStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>A tax form milestone. Its code is unique within a tenant.</summary>
public sealed class TaxFormMilestone(string code, string title, string channel, double minimum, double maximum, string location, string externalRef, double ratio, bool visible, TaxFormMilestoneStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Channel { get; set; } = channel;
    public double Minimum { get; set; } = minimum;
    public double Maximum { get; set; } = maximum;
    public string Location { get; set; } = location;
    public string ExternalRef { get; set; } = externalRef;
    public double Ratio { get; set; } = ratio;
    public bool Visible { get; set; } = visible;
    public TaxFormMilestoneStatus Status { get; set; } = status;

    /// <summary>Starts a tax form milestone in status planned.</summary>
    public static TaxFormMilestone NewTaxFormMilestone(string code, string title)
    {
        return new TaxFormMilestone(code, title, "", 0.0, 0.0, "", "", 0.0, false, TaxFormMilestoneStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case TaxFormMilestoneStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(TaxFormMilestoneStatus next)
    {
        switch (Status)
        {
            case TaxFormMilestoneStatus.Planned:
                return next == TaxFormMilestoneStatus.Completed || next == TaxFormMilestoneStatus.Cancelled;
            case TaxFormMilestoneStatus.Scheduled:
                return next == TaxFormMilestoneStatus.InProgress || next == TaxFormMilestoneStatus.Cancelled;
            case TaxFormMilestoneStatus.InProgress:
                return next == TaxFormMilestoneStatus.Completed || next == TaxFormMilestoneStatus.Cancelled;
            case TaxFormMilestoneStatus.Completed:
                return next == TaxFormMilestoneStatus.Cancelled;
        }
        return false;
    }

    public void MoveTo(TaxFormMilestoneStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"tax form milestone {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} [{Title}]";
    }

    public double WeightedValue()
    {
        return Math.Round(Ratio * Maximum, 2);
    }

    public TaxFormMilestone Copy()
    {
        return new TaxFormMilestone(
            Code,
            Title,
            Channel,
            Minimum,
            Maximum,
            Location,
            ExternalRef,
            Ratio,
            Visible,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Channel);
    }
}
