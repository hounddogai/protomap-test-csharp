namespace Acme.Workforce.Contractors.Vendor.DefaultVendorSchedule;

using Acme.Workforce.Kit.Text;

/// <summary>Where a weighted vendor schedule is in its life cycle.</summary>
public enum WeightedVendorScheduleStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>A weighted vendor schedule as the vendor team keeps it.</summary>
public sealed class WeightedVendorSchedule(string code, string name, double weight, int batchSize, double score, bool visible, string employeeId, WeightedVendorScheduleStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public double Weight { get; set; } = weight;
    public int BatchSize { get; set; } = batchSize;
    public double Score { get; set; } = score;
    public bool Visible { get; set; } = visible;
    public string EmployeeId { get; set; } = employeeId;
    public WeightedVendorScheduleStatus Status { get; set; } = status;

    /// <summary>Starts a weighted vendor schedule in status created.</summary>
    public static WeightedVendorSchedule NewWeightedVendorSchedule(string code, string name)
    {
        return new WeightedVendorSchedule(code, name, 0.0, 0, 0.0, false, "", WeightedVendorScheduleStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case WeightedVendorScheduleStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(WeightedVendorScheduleStatus next)
    {
        switch (Status)
        {
            case WeightedVendorScheduleStatus.Created:
                switch (next)
                {
                    case WeightedVendorScheduleStatus.Validated:
                    case WeightedVendorScheduleStatus.Retired:
                        return true;
                }
                return false;
            case WeightedVendorScheduleStatus.Validated:
                return next == WeightedVendorScheduleStatus.Retired;
            case WeightedVendorScheduleStatus.Published:
                switch (next)
                {
                    case WeightedVendorScheduleStatus.Retired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(WeightedVendorScheduleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"weighted vendor schedule {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} / {Name}";
    }

    public double WeightedExposure()
    {
        return Math.Round((double)BatchSize - Score, 2);
    }

    public WeightedVendorSchedule Copy()
    {
        return new WeightedVendorSchedule(Code, Name, Weight, BatchSize, Score, Visible, EmployeeId, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
