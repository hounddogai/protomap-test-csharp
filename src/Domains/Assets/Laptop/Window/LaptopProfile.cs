namespace Acme.Workforce.Assets.Laptop.Window;

using Acme.Workforce.Kit.Text;

/// <summary>Where a laptop profile is in its life cycle.</summary>
public enum LaptopProfileStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>One laptop profile, with what the planners need to act on it.</summary>
public sealed class LaptopProfile(string code, string title, string channel, int durationMinutes, double floorValue, double discount, string costCenter, LaptopProfileStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Channel { get; set; } = channel;
    public int DurationMinutes { get; set; } = durationMinutes;
    public double FloorValue { get; set; } = floorValue;
    public double Discount { get; set; } = discount;
    public string CostCenter { get; set; } = costCenter;
    public LaptopProfileStatus Status { get; set; } = status;

    /// <summary>Starts a laptop profile in status open.</summary>
    public static LaptopProfile NewLaptopProfile(string code, string title)
    {
        return new LaptopProfile(code, title, "", 0, 0.0, 0.0, "", LaptopProfileStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LaptopProfileStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LaptopProfileStatus next)
    {
        switch (Status)
        {
            case LaptopProfileStatus.Open:
                switch (next)
                {
                    case LaptopProfileStatus.Resolved:
                    case LaptopProfileStatus.Reopened:
                        return true;
                }
                return false;
            case LaptopProfileStatus.InReview:
                return next == LaptopProfileStatus.Resolved;
            case LaptopProfileStatus.Resolved:
                switch (next)
                {
                    case LaptopProfileStatus.Reopened:
                    case LaptopProfileStatus.Archived:
                        return true;
                }
                return false;
            case LaptopProfileStatus.Reopened:
                return next == LaptopProfileStatus.Archived;
        }
        return false;
    }

    public void MoveTo(LaptopProfileStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"laptop profile {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} ({Title})";
    }

    public double AdjustedLoad()
    {
        return Math.Round(Discount + FloorValue, 2);
    }

    public LaptopProfile Copy()
    {
        return new LaptopProfile(Code, Title, Channel, DurationMinutes, FloorValue, Discount, CostCenter, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Channel);
    }
}
