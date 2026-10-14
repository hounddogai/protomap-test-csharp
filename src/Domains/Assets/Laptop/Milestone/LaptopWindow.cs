namespace Acme.Workforce.Assets.Laptop.Milestone;

using Acme.Workforce.Kit.Text;

/// <summary>Where a laptop window is in its life cycle.</summary>
public enum LaptopWindowStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>A laptop window. Its code is unique within a tenant.</summary>
public sealed class LaptopWindow(string code, string name, bool approved, string contactEmail, int tenureMonths, double margin, string grade, double rating, string badge, string employeeId, LaptopWindowStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public bool Approved { get; set; } = approved;
    public string ContactEmail { get; set; } = contactEmail;
    public int TenureMonths { get; set; } = tenureMonths;
    public double Margin { get; set; } = margin;
    public string Grade { get; set; } = grade;
    public double Rating { get; set; } = rating;
    public string Badge { get; set; } = badge;
    public string EmployeeId { get; set; } = employeeId;
    public LaptopWindowStatus Status { get; set; } = status;

    /// <summary>Starts a laptop window in status requested.</summary>
    public static LaptopWindow NewLaptopWindow(string code, string name)
    {
        return new LaptopWindow(code, name, false, "", 0, 0.0, "", 0.0, "", "", LaptopWindowStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LaptopWindowStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LaptopWindowStatus next)
    {
        switch (Status)
        {
            case LaptopWindowStatus.Requested:
                switch (next)
                {
                    case LaptopWindowStatus.Expired:
                        return true;
                }
                return false;
            case LaptopWindowStatus.Granted:
                return next == LaptopWindowStatus.Revoked || next == LaptopWindowStatus.Expired;
            case LaptopWindowStatus.Revoked:
                return next == LaptopWindowStatus.Expired;
        }
        return false;
    }

    public void MoveTo(LaptopWindowStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"laptop window {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} [{Name}]";
    }

    public double AdjustedValue()
    {
        return Math.Round((double)TenureMonths - Margin, 2);
    }

    public LaptopWindow Copy()
    {
        return new LaptopWindow(
            Code,
            Name,
            Approved,
            ContactEmail,
            TenureMonths,
            Margin,
            Grade,
            Rating,
            Badge,
            EmployeeId,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
