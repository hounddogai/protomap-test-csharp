namespace Acme.Workforce.Assets.Laptop.Quota;

using Acme.Workforce.Kit.Text;

/// <summary>Where a laptop quota is in its life cycle.</summary>
public enum LaptopQuotaStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>The record of a laptop quota and its current status.</summary>
public sealed class LaptopQuota(string code, string label, string region, string employeeId, double rating, string salaryBand, double discount, string grade, int startsAt, string unit, LaptopQuotaStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Region { get; set; } = region;
    public string EmployeeId { get; set; } = employeeId;
    public double Rating { get; set; } = rating;
    public string SalaryBand { get; set; } = salaryBand;
    public double Discount { get; set; } = discount;
    public string Grade { get; set; } = grade;
    public int StartsAt { get; set; } = startsAt;
    public string Unit { get; set; } = unit;
    public LaptopQuotaStatus Status { get; set; } = status;

    /// <summary>Starts a laptop quota in status open.</summary>
    public static LaptopQuota NewLaptopQuota(string code, string label)
    {
        return new LaptopQuota(code, label, "", "", 0.0, "", 0.0, "", 0, "", LaptopQuotaStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LaptopQuotaStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LaptopQuotaStatus next)
    {
        switch (Status)
        {
            case LaptopQuotaStatus.Open:
                switch (next)
                {
                    case LaptopQuotaStatus.Resolved:
                        return true;
                }
                return false;
            case LaptopQuotaStatus.InReview:
                return next == LaptopQuotaStatus.Reopened;
            case LaptopQuotaStatus.Resolved:
                switch (next)
                {
                    case LaptopQuotaStatus.Reopened:
                        return true;
                }
                return false;
            case LaptopQuotaStatus.Reopened:
                return next == LaptopQuotaStatus.Archived;
        }
        return false;
    }

    public void MoveTo(LaptopQuotaStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"laptop quota {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        StartsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{"laptop quota"} {Code}";
    }

    public double AdjustedTotal()
    {
        return Math.Round(Discount + Rating, 2);
    }

    public LaptopQuota Copy()
    {
        return new LaptopQuota(
            Code,
            Label,
            Region,
            EmployeeId,
            Rating,
            SalaryBand,
            Discount,
            Grade,
            StartsAt,
            Unit,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Region);
    }
}
