namespace Acme.Workforce.Assets.Laptop.Forecast;

using Acme.Workforce.Kit.Text;

/// <summary>Where a laptop baseline is in its life cycle.</summary>
public enum LaptopBaselineStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>A laptop baseline. Its code is unique within a tenant.</summary>
public sealed class LaptopBaseline(string code, string label, string grade, string name, double ceiling, string contactEmail, string employeeId, double rate, string checksum, LaptopBaselineStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Grade { get; set; } = grade;
    public string Name { get; set; } = name;
    public double Ceiling { get; set; } = ceiling;
    public string ContactEmail { get; set; } = contactEmail;
    public string EmployeeId { get; set; } = employeeId;
    public double Rate { get; set; } = rate;
    public string Checksum { get; set; } = checksum;
    public LaptopBaselineStatus Status { get; set; } = status;

    /// <summary>Starts a laptop baseline in status planned.</summary>
    public static LaptopBaseline NewLaptopBaseline(string code, string label)
    {
        return new LaptopBaseline(code, label, "", "", 0.0, "", "", 0.0, "", LaptopBaselineStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LaptopBaselineStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LaptopBaselineStatus next)
    {
        switch (Status)
        {
            case LaptopBaselineStatus.Planned:
                switch (next)
                {
                    case LaptopBaselineStatus.Scheduled:
                    case LaptopBaselineStatus.Cancelled:
                        return true;
                }
                return false;
            case LaptopBaselineStatus.Scheduled:
                switch (next)
                {
                    case LaptopBaselineStatus.InProgress:
                    case LaptopBaselineStatus.Completed:
                        return true;
                }
                return false;
            case LaptopBaselineStatus.InProgress:
                switch (next)
                {
                    case LaptopBaselineStatus.Completed:
                    case LaptopBaselineStatus.Cancelled:
                        return true;
                }
                return false;
            case LaptopBaselineStatus.Completed:
                return next == LaptopBaselineStatus.Cancelled;
        }
        return false;
    }

    public void MoveTo(LaptopBaselineStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"laptop baseline {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} [{Label}]";
    }

    public double WeightedLoad()
    {
        return Math.Round(Ceiling - Rate, 2);
    }

    public LaptopBaseline Copy()
    {
        return new LaptopBaseline(
            Code,
            Label,
            Grade,
            Name,
            Ceiling,
            ContactEmail,
            EmployeeId,
            Rate,
            Checksum,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
