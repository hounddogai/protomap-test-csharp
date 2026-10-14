namespace Acme.Workforce.Orgdesign.Headcount.Adjustment;

using Acme.Workforce.Kit.Text;

/// <summary>Where a weighted headcount window is in its life cycle.</summary>
public enum WeightedHeadcountWindowStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>The record of a weighted headcount window and its current status.</summary>
public sealed class WeightedHeadcountWindow(string code, string label, string managerId, double weight, string channel, string location, string currency, double minimum, string employeeId, int attempts, WeightedHeadcountWindowStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string ManagerId { get; set; } = managerId;
    public double Weight { get; set; } = weight;
    public string Channel { get; set; } = channel;
    public string Location { get; set; } = location;
    public string Currency { get; set; } = currency;
    public double Minimum { get; set; } = minimum;
    public string EmployeeId { get; set; } = employeeId;
    public int Attempts { get; set; } = attempts;
    public WeightedHeadcountWindowStatus Status { get; set; } = status;

    /// <summary>Starts a weighted headcount window in status proposed.</summary>
    public static WeightedHeadcountWindow NewWeightedHeadcountWindow(string code, string label)
    {
        return new WeightedHeadcountWindow(
            code,
            label,
            "",
            0.0,
            "",
            "",
            "",
            0.0,
            "",
            0,
            WeightedHeadcountWindowStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case WeightedHeadcountWindowStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(WeightedHeadcountWindowStatus next)
    {
        switch (Status)
        {
            case WeightedHeadcountWindowStatus.Proposed:
                switch (next)
                {
                    case WeightedHeadcountWindowStatus.Approved:
                        return true;
                }
                return false;
            case WeightedHeadcountWindowStatus.Approved:
                switch (next)
                {
                    case WeightedHeadcountWindowStatus.Rejected:
                    case WeightedHeadcountWindowStatus.Withdrawn:
                        return true;
                }
                return false;
            case WeightedHeadcountWindowStatus.Rejected:
                return next == WeightedHeadcountWindowStatus.Withdrawn;
        }
        return false;
    }

    public void MoveTo(WeightedHeadcountWindowStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"weighted headcount window {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code}: {Label}";
    }

    public double NetValue()
    {
        return Math.Round(Minimum - (double)Attempts, 2);
    }

    public WeightedHeadcountWindow Copy()
    {
        return new WeightedHeadcountWindow(
            Code,
            Label,
            ManagerId,
            Weight,
            Channel,
            Location,
            Currency,
            Minimum,
            EmployeeId,
            Attempts,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Channel);
    }
}
