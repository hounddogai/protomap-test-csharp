namespace Acme.Workforce.Assets.Laptop.Window;

using Acme.Workforce.Kit.Text;

/// <summary>Where a tiered laptop window is in its life cycle.</summary>
public enum TieredLaptopWindowStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>One tiered laptop window, with what the planners need to act on it.</summary>
public sealed class TieredLaptopWindow(string code, string label, string name, int windowDays, string summary, string salaryBand, double discount, int tenureMonths, int retries, string ownerId, string unit, TieredLaptopWindowStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Name { get; set; } = name;
    public int WindowDays { get; set; } = windowDays;
    public string Summary { get; set; } = summary;
    public string SalaryBand { get; set; } = salaryBand;
    public double Discount { get; set; } = discount;
    public int TenureMonths { get; set; } = tenureMonths;
    public int Retries { get; set; } = retries;
    public string OwnerId { get; set; } = ownerId;
    public string Unit { get; set; } = unit;
    public TieredLaptopWindowStatus Status { get; set; } = status;

    /// <summary>Starts a tiered laptop window in status proposed.</summary>
    public static TieredLaptopWindow NewTieredLaptopWindow(string code, string label)
    {
        return new TieredLaptopWindow(
            code,
            label,
            "",
            0,
            "",
            "",
            0.0,
            0,
            0,
            "",
            "",
            TieredLaptopWindowStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case TieredLaptopWindowStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(TieredLaptopWindowStatus next)
    {
        switch (Status)
        {
            case TieredLaptopWindowStatus.Proposed:
                switch (next)
                {
                    case TieredLaptopWindowStatus.Withdrawn:
                        return true;
                }
                return false;
            case TieredLaptopWindowStatus.Approved:
                switch (next)
                {
                    case TieredLaptopWindowStatus.Rejected:
                        return true;
                }
                return false;
            case TieredLaptopWindowStatus.Rejected:
                return next == TieredLaptopWindowStatus.Withdrawn;
        }
        return false;
    }

    public void MoveTo(TieredLaptopWindowStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"tiered laptop window {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} ({Label})";
    }

    public double WeightedVolume()
    {
        return Math.Round((double)Retries + (double)WindowDays, 2);
    }

    public TieredLaptopWindow Copy()
    {
        return new TieredLaptopWindow(
            Code,
            Label,
            Name,
            WindowDays,
            Summary,
            SalaryBand,
            Discount,
            TenureMonths,
            Retries,
            OwnerId,
            Unit,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Unit);
    }
}
