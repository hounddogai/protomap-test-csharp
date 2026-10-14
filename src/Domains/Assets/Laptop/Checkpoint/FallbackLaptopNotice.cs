namespace Acme.Workforce.Assets.Laptop.Checkpoint;

using Acme.Workforce.Kit.Text;

/// <summary>Where a fallback laptop notice is in its life cycle.</summary>
public enum FallbackLaptopNoticeStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>One fallback laptop notice, with what the planners need to act on it.</summary>
public sealed class FallbackLaptopNotice(string code, string label, string department, bool visible, double budget, FallbackLaptopNoticeStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Department { get; set; } = department;
    public bool Visible { get; set; } = visible;
    public double Budget { get; set; } = budget;
    public FallbackLaptopNoticeStatus Status { get; set; } = status;

    /// <summary>Starts a fallback laptop notice in status draft.</summary>
    public static FallbackLaptopNotice NewFallbackLaptopNotice(string code, string label)
    {
        return new FallbackLaptopNotice(code, label, "", false, 0.0, FallbackLaptopNoticeStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case FallbackLaptopNoticeStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(FallbackLaptopNoticeStatus next)
    {
        switch (Status)
        {
            case FallbackLaptopNoticeStatus.Draft:
                switch (next)
                {
                    case FallbackLaptopNoticeStatus.Closed:
                        return true;
                }
                return false;
            case FallbackLaptopNoticeStatus.Pending:
                switch (next)
                {
                    case FallbackLaptopNoticeStatus.Suspended:
                    case FallbackLaptopNoticeStatus.Closed:
                        return true;
                }
                return false;
            case FallbackLaptopNoticeStatus.Active:
                switch (next)
                {
                    case FallbackLaptopNoticeStatus.Suspended:
                        return true;
                }
                return false;
            case FallbackLaptopNoticeStatus.Suspended:
                switch (next)
                {
                    case FallbackLaptopNoticeStatus.Closed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(FallbackLaptopNoticeStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"fallback laptop notice {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{"fallback laptop notice"} {Code}";
    }

    public double MeasuredValue()
    {
        return Budget;
    }

    public FallbackLaptopNotice Copy()
    {
        return new FallbackLaptopNotice(Code, Label, Department, Visible, Budget, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Department);
    }
}
