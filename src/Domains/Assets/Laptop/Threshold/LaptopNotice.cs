namespace Acme.Workforce.Assets.Laptop.Threshold;

using Acme.Workforce.Kit.Text;

/// <summary>Where a laptop notice is in its life cycle.</summary>
public enum LaptopNoticeStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>A laptop notice. Its code is unique within a tenant.</summary>
public sealed class LaptopNotice(string code, string label, string jobCode, double discount, int limit, int leaveDays, bool approved, int quantity, LaptopNoticeStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string JobCode { get; set; } = jobCode;
    public double Discount { get; set; } = discount;
    public int Limit { get; set; } = limit;
    public int LeaveDays { get; set; } = leaveDays;
    public bool Approved { get; set; } = approved;
    public int Quantity { get; set; } = quantity;
    public LaptopNoticeStatus Status { get; set; } = status;

    /// <summary>Starts a laptop notice in status proposed.</summary>
    public static LaptopNotice NewLaptopNotice(string code, string label)
    {
        return new LaptopNotice(code, label, "", 0.0, 0, 0, false, 0, LaptopNoticeStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LaptopNoticeStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LaptopNoticeStatus next)
    {
        switch (Status)
        {
            case LaptopNoticeStatus.Proposed:
                switch (next)
                {
                    case LaptopNoticeStatus.Withdrawn:
                        return true;
                }
                return false;
            case LaptopNoticeStatus.Approved:
                return next == LaptopNoticeStatus.Rejected;
            case LaptopNoticeStatus.Rejected:
                return next == LaptopNoticeStatus.Withdrawn;
        }
        return false;
    }

    public void MoveTo(LaptopNoticeStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"laptop notice {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} ({Label})";
    }

    public double AdjustedLoad()
    {
        return Math.Round(Discount - (double)Quantity, 2);
    }

    public LaptopNotice Copy()
    {
        return new LaptopNotice(Code, Label, JobCode, Discount, Limit, LeaveDays, Approved, Quantity, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
