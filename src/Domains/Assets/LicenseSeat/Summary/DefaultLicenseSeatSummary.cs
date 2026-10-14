namespace Acme.Workforce.Assets.LicenseSeat.Summary;

using Acme.Workforce.Kit.Text;

/// <summary>Where a default license seat summary is in its life cycle.</summary>
public enum DefaultLicenseSeatSummaryStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>The record of a default license seat summary and its current status.</summary>
public sealed class DefaultLicenseSeatSummary(string code, string title, bool active, double weight, string notes, string location, string segment, double rate, string badge, string employeeId, DefaultLicenseSeatSummaryStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public bool Active { get; set; } = active;
    public double Weight { get; set; } = weight;
    public string Notes { get; set; } = notes;
    public string Location { get; set; } = location;
    public string Segment { get; set; } = segment;
    public double Rate { get; set; } = rate;
    public string Badge { get; set; } = badge;
    public string EmployeeId { get; set; } = employeeId;
    public DefaultLicenseSeatSummaryStatus Status { get; set; } = status;

    /// <summary>Starts a default license seat summary in status proposed.</summary>
    public static DefaultLicenseSeatSummary NewDefaultLicenseSeatSummary(string code, string title)
    {
        return new DefaultLicenseSeatSummary(
            code,
            title,
            false,
            0.0,
            "",
            "",
            "",
            0.0,
            "",
            "",
            DefaultLicenseSeatSummaryStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case DefaultLicenseSeatSummaryStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(DefaultLicenseSeatSummaryStatus next)
    {
        switch (Status)
        {
            case DefaultLicenseSeatSummaryStatus.Proposed:
                switch (next)
                {
                    case DefaultLicenseSeatSummaryStatus.Approved:
                        return true;
                }
                return false;
            case DefaultLicenseSeatSummaryStatus.Approved:
                switch (next)
                {
                    case DefaultLicenseSeatSummaryStatus.Withdrawn:
                        return true;
                }
                return false;
            case DefaultLicenseSeatSummaryStatus.Rejected:
                switch (next)
                {
                    case DefaultLicenseSeatSummaryStatus.Withdrawn:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(DefaultLicenseSeatSummaryStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"default license seat summary {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"default license seat summary"} {Code}";
    }

    public double ProjectedValue()
    {
        return Math.Round(Rate * Weight, 2);
    }

    public DefaultLicenseSeatSummary Copy()
    {
        return new DefaultLicenseSeatSummary(
            Code,
            Title,
            Active,
            Weight,
            Notes,
            Location,
            Segment,
            Rate,
            Badge,
            EmployeeId,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Segment);
    }
}
