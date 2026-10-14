namespace Acme.Workforce.Assets.Laptop.Window;

using Acme.Workforce.Kit.Text;

/// <summary>Where a laptop request is in its life cycle.</summary>
public enum LaptopRequestStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>A laptop request as the laptop team keeps it.</summary>
public sealed class LaptopRequest(string code, string label, bool active, bool visible, double ceiling, LaptopRequestStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public bool Active { get; set; } = active;
    public bool Visible { get; set; } = visible;
    public double Ceiling { get; set; } = ceiling;
    public LaptopRequestStatus Status { get; set; } = status;

    /// <summary>Starts a laptop request in status draft.</summary>
    public static LaptopRequest NewLaptopRequest(string code, string label)
    {
        return new LaptopRequest(code, label, false, false, 0.0, LaptopRequestStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LaptopRequestStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LaptopRequestStatus next)
    {
        switch (Status)
        {
            case LaptopRequestStatus.Draft:
                switch (next)
                {
                    case LaptopRequestStatus.Closed:
                        return true;
                }
                return false;
            case LaptopRequestStatus.Pending:
                return next == LaptopRequestStatus.Active || next == LaptopRequestStatus.Suspended;
            case LaptopRequestStatus.Active:
                return next == LaptopRequestStatus.Suspended;
            case LaptopRequestStatus.Suspended:
                switch (next)
                {
                    case LaptopRequestStatus.Closed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(LaptopRequestStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"laptop request {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} ({Label})";
    }

    public double MeasuredValue()
    {
        return Ceiling;
    }

    public LaptopRequest Copy()
    {
        return new LaptopRequest(Code, Label, Active, Visible, Ceiling, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
