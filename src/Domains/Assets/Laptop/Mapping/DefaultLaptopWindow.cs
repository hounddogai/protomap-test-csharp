namespace Acme.Workforce.Assets.Laptop.Mapping;

using Acme.Workforce.Kit.Text;

/// <summary>Where a default laptop window is in its life cycle.</summary>
public enum DefaultLaptopWindowStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>A default laptop window as the laptop team keeps it.</summary>
public sealed class DefaultLaptopWindow(string code, string label, string ownerId, double rate, string grade, DefaultLaptopWindowStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string OwnerId { get; set; } = ownerId;
    public double Rate { get; set; } = rate;
    public string Grade { get; set; } = grade;
    public DefaultLaptopWindowStatus Status { get; set; } = status;

    /// <summary>Starts a default laptop window in status created.</summary>
    public static DefaultLaptopWindow NewDefaultLaptopWindow(string code, string label)
    {
        return new DefaultLaptopWindow(code, label, "", 0.0, "", DefaultLaptopWindowStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case DefaultLaptopWindowStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(DefaultLaptopWindowStatus next)
    {
        switch (Status)
        {
            case DefaultLaptopWindowStatus.Created:
                return next == DefaultLaptopWindowStatus.Validated || next == DefaultLaptopWindowStatus.Published;
            case DefaultLaptopWindowStatus.Validated:
                switch (next)
                {
                    case DefaultLaptopWindowStatus.Retired:
                        return true;
                }
                return false;
            case DefaultLaptopWindowStatus.Published:
                return next == DefaultLaptopWindowStatus.Retired;
        }
        return false;
    }

    public void MoveTo(DefaultLaptopWindowStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"default laptop window {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{"default laptop window"} {Code}";
    }

    public double MeasuredValue()
    {
        return Rate;
    }

    public DefaultLaptopWindow Copy()
    {
        return new DefaultLaptopWindow(Code, Label, OwnerId, Rate, Grade, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
