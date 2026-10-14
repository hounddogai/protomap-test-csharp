namespace Acme.Workforce.Absence.Holiday.Adjustment;

using Acme.Workforce.Kit.Text;

/// <summary>Where a holiday group is in its life cycle.</summary>
public enum HolidayGroupStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>A holiday group as the holiday team keeps it.</summary>
public sealed class HolidayGroup(string code, string name, string title, int quantity, double budget, string segment, string jobCode, string salaryBand, string label, int tenureMonths, string checksum, HolidayGroupStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Title { get; set; } = title;
    public int Quantity { get; set; } = quantity;
    public double Budget { get; set; } = budget;
    public string Segment { get; set; } = segment;
    public string JobCode { get; set; } = jobCode;
    public string SalaryBand { get; set; } = salaryBand;
    public string Label { get; set; } = label;
    public int TenureMonths { get; set; } = tenureMonths;
    public string Checksum { get; set; } = checksum;
    public HolidayGroupStatus Status { get; set; } = status;

    /// <summary>Starts a holiday group in status created.</summary>
    public static HolidayGroup NewHolidayGroup(string code, string name)
    {
        return new HolidayGroup(code, name, "", 0, 0.0, "", "", "", "", 0, "", HolidayGroupStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case HolidayGroupStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(HolidayGroupStatus next)
    {
        switch (Status)
        {
            case HolidayGroupStatus.Created:
                switch (next)
                {
                    case HolidayGroupStatus.Published:
                    case HolidayGroupStatus.Retired:
                        return true;
                }
                return false;
            case HolidayGroupStatus.Validated:
                switch (next)
                {
                    case HolidayGroupStatus.Published:
                        return true;
                }
                return false;
            case HolidayGroupStatus.Published:
                switch (next)
                {
                    case HolidayGroupStatus.Retired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(HolidayGroupStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"holiday group {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{"holiday group"} {Code}";
    }

    public double ProjectedVolume()
    {
        return Math.Round((double)TenureMonths - Budget, 2);
    }

    public HolidayGroup Copy()
    {
        return new HolidayGroup(
            Code,
            Name,
            Title,
            Quantity,
            Budget,
            Segment,
            JobCode,
            SalaryBand,
            Label,
            TenureMonths,
            Checksum,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Segment);
    }
}
