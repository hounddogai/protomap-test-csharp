namespace Acme.Workforce.Assets.LicenseSeat.Bundle;

using Acme.Workforce.Kit.Text;

/// <summary>Where a weighted license seat bundle is in its life cycle.</summary>
public enum WeightedLicenseSeatBundleStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>A weighted license seat bundle as the license seat team keeps it.</summary>
public sealed class WeightedLicenseSeatBundle(string code, string label, int revision, double floorValue, double budget, string unit, int windowDays, int capacity, string grade, double minimum, string employeeId, WeightedLicenseSeatBundleStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int Revision { get; set; } = revision;
    public double FloorValue { get; set; } = floorValue;
    public double Budget { get; set; } = budget;
    public string Unit { get; set; } = unit;
    public int WindowDays { get; set; } = windowDays;
    public int Capacity { get; set; } = capacity;
    public string Grade { get; set; } = grade;
    public double Minimum { get; set; } = minimum;
    public string EmployeeId { get; set; } = employeeId;
    public WeightedLicenseSeatBundleStatus Status { get; set; } = status;

    /// <summary>Starts a weighted license seat bundle in status proposed.</summary>
    public static WeightedLicenseSeatBundle NewWeightedLicenseSeatBundle(string code, string label)
    {
        return new WeightedLicenseSeatBundle(
            code,
            label,
            0,
            0.0,
            0.0,
            "",
            0,
            0,
            "",
            0.0,
            "",
            WeightedLicenseSeatBundleStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case WeightedLicenseSeatBundleStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(WeightedLicenseSeatBundleStatus next)
    {
        switch (Status)
        {
            case WeightedLicenseSeatBundleStatus.Proposed:
                return next == WeightedLicenseSeatBundleStatus.Approved || next == WeightedLicenseSeatBundleStatus.Rejected;
            case WeightedLicenseSeatBundleStatus.Approved:
                switch (next)
                {
                    case WeightedLicenseSeatBundleStatus.Withdrawn:
                        return true;
                }
                return false;
            case WeightedLicenseSeatBundleStatus.Rejected:
                return next == WeightedLicenseSeatBundleStatus.Withdrawn;
        }
        return false;
    }

    public void MoveTo(WeightedLicenseSeatBundleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"weighted license seat bundle {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Revision += 1;
    }

    public string DisplayLabel()
    {
        return $"{"weighted license seat bundle"} {Code}";
    }

    public double AdjustedTotal()
    {
        return Math.Round((double)WindowDays - (double)Revision, 2);
    }

    public WeightedLicenseSeatBundle Copy()
    {
        return new WeightedLicenseSeatBundle(
            Code,
            Label,
            Revision,
            FloorValue,
            Budget,
            Unit,
            WindowDays,
            Capacity,
            Grade,
            Minimum,
            EmployeeId,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Unit);
    }
}
