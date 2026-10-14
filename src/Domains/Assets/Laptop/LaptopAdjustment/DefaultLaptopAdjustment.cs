namespace Acme.Workforce.Assets.Laptop.LaptopAdjustment;

using Acme.Workforce.Kit.Text;

/// <summary>Where a default laptop adjustment is in its life cycle.</summary>
public enum DefaultLaptopAdjustmentStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>One default laptop adjustment, with what the planners need to act on it.</summary>
public sealed class DefaultLaptopAdjustment(string code, string label, bool pinned, int expiresAt, string currency, DefaultLaptopAdjustmentStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public bool Pinned { get; set; } = pinned;
    public int ExpiresAt { get; set; } = expiresAt;
    public string Currency { get; set; } = currency;
    public DefaultLaptopAdjustmentStatus Status { get; set; } = status;

    /// <summary>Starts a default laptop adjustment in status draft.</summary>
    public static DefaultLaptopAdjustment NewDefaultLaptopAdjustment(string code, string label)
    {
        return new DefaultLaptopAdjustment(code, label, false, 0, "", DefaultLaptopAdjustmentStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case DefaultLaptopAdjustmentStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(DefaultLaptopAdjustmentStatus next)
    {
        switch (Status)
        {
            case DefaultLaptopAdjustmentStatus.Draft:
                switch (next)
                {
                    case DefaultLaptopAdjustmentStatus.Pending:
                        return true;
                }
                return false;
            case DefaultLaptopAdjustmentStatus.Pending:
                switch (next)
                {
                    case DefaultLaptopAdjustmentStatus.Closed:
                        return true;
                }
                return false;
            case DefaultLaptopAdjustmentStatus.Active:
                return next == DefaultLaptopAdjustmentStatus.Suspended || next == DefaultLaptopAdjustmentStatus.Closed;
            case DefaultLaptopAdjustmentStatus.Suspended:
                switch (next)
                {
                    case DefaultLaptopAdjustmentStatus.Closed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(DefaultLaptopAdjustmentStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"default laptop adjustment {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        ExpiresAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{Code} / {Label}";
    }

    public double MeasuredValue()
    {
        return (double)ExpiresAt;
    }

    public DefaultLaptopAdjustment Copy()
    {
        return new DefaultLaptopAdjustment(Code, Label, Pinned, ExpiresAt, Currency, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
