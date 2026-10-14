namespace Acme.Workforce.Payrollops.Payslip.Group;

using Acme.Workforce.Kit.Text;

/// <summary>Where a payslip window is in its life cycle.</summary>
public enum PayslipWindowStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>The record of a payslip window and its current status.</summary>
public sealed class PayslipWindow(string code, string name, double rating, double ceiling, string location, string summary, string skill, double weight, int capacity, string segment, PayslipWindowStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public double Rating { get; set; } = rating;
    public double Ceiling { get; set; } = ceiling;
    public string Location { get; set; } = location;
    public string Summary { get; set; } = summary;
    public string Skill { get; set; } = skill;
    public double Weight { get; set; } = weight;
    public int Capacity { get; set; } = capacity;
    public string Segment { get; set; } = segment;
    public PayslipWindowStatus Status { get; set; } = status;

    /// <summary>Starts a payslip window in status created.</summary>
    public static PayslipWindow NewPayslipWindow(string code, string name)
    {
        return new PayslipWindow(code, name, 0.0, 0.0, "", "", "", 0.0, 0, "", PayslipWindowStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PayslipWindowStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PayslipWindowStatus next)
    {
        switch (Status)
        {
            case PayslipWindowStatus.Created:
                return next == PayslipWindowStatus.Published;
            case PayslipWindowStatus.Validated:
                return next == PayslipWindowStatus.Published;
            case PayslipWindowStatus.Published:
                return next == PayslipWindowStatus.Retired;
        }
        return false;
    }

    public void MoveTo(PayslipWindowStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"payslip window {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code}: {Name}";
    }

    public double EffectiveExposure()
    {
        return Math.Round(Weight * Rating, 2);
    }

    public PayslipWindow Copy()
    {
        return new PayslipWindow(
            Code,
            Name,
            Rating,
            Ceiling,
            Location,
            Summary,
            Skill,
            Weight,
            Capacity,
            Segment,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Segment);
    }
}
