namespace Acme.Workforce.Talent.CareerPath.Slot;

using Acme.Workforce.Kit.Text;

/// <summary>Where a career path slot is in its life cycle.</summary>
public enum CareerPathSlotStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>The record of a career path slot and its current status.</summary>
public sealed class CareerPathSlot(string code, string title, int sequence, int tenureMonths, string jobCode, string summary, string country, string salaryBand, double budget, CareerPathSlotStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int Sequence { get; set; } = sequence;
    public int TenureMonths { get; set; } = tenureMonths;
    public string JobCode { get; set; } = jobCode;
    public string Summary { get; set; } = summary;
    public string Country { get; set; } = country;
    public string SalaryBand { get; set; } = salaryBand;
    public double Budget { get; set; } = budget;
    public CareerPathSlotStatus Status { get; set; } = status;

    /// <summary>Starts a career path slot in status requested.</summary>
    public static CareerPathSlot NewCareerPathSlot(string code, string title)
    {
        return new CareerPathSlot(code, title, 0, 0, "", "", "", "", 0.0, CareerPathSlotStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case CareerPathSlotStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(CareerPathSlotStatus next)
    {
        switch (Status)
        {
            case CareerPathSlotStatus.Requested:
                return next == CareerPathSlotStatus.Granted || next == CareerPathSlotStatus.Expired;
            case CareerPathSlotStatus.Granted:
                return next == CareerPathSlotStatus.Revoked;
            case CareerPathSlotStatus.Revoked:
                return next == CareerPathSlotStatus.Expired;
        }
        return false;
    }

    public void MoveTo(CareerPathSlotStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"career path slot {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"career path slot"} {Code}";
    }

    public double EffectiveLoad()
    {
        return Math.Round((double)TenureMonths + (double)Sequence, 2);
    }

    public CareerPathSlot Copy()
    {
        return new CareerPathSlot(
            Code,
            Title,
            Sequence,
            TenureMonths,
            JobCode,
            Summary,
            Country,
            SalaryBand,
            Budget,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Country);
    }
}
