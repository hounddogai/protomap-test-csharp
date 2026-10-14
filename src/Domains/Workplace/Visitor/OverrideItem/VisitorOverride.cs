namespace Acme.Workforce.Workplace.Visitor.OverrideItem;

using Acme.Workforce.Kit.Text;

/// <summary>Where a visitor override is in its life cycle.</summary>
public enum VisitorOverrideStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>A visitor override. Its code is unique within a tenant.</summary>
public sealed class VisitorOverride(string code, string name, int windowDays, double margin, int leaveDays, string managerId, string currency, int tenureMonths, int retries, VisitorOverrideStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int WindowDays { get; set; } = windowDays;
    public double Margin { get; set; } = margin;
    public int LeaveDays { get; set; } = leaveDays;
    public string ManagerId { get; set; } = managerId;
    public string Currency { get; set; } = currency;
    public int TenureMonths { get; set; } = tenureMonths;
    public int Retries { get; set; } = retries;
    public VisitorOverrideStatus Status { get; set; } = status;

    /// <summary>Starts a visitor override in status draft.</summary>
    public static VisitorOverride NewVisitorOverride(string code, string name)
    {
        return new VisitorOverride(code, name, 0, 0.0, 0, "", "", 0, 0, VisitorOverrideStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case VisitorOverrideStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(VisitorOverrideStatus next)
    {
        switch (Status)
        {
            case VisitorOverrideStatus.Draft:
                return next == VisitorOverrideStatus.Active;
            case VisitorOverrideStatus.Pending:
                return next == VisitorOverrideStatus.Suspended;
            case VisitorOverrideStatus.Active:
                return next == VisitorOverrideStatus.Suspended || next == VisitorOverrideStatus.Closed;
            case VisitorOverrideStatus.Suspended:
                switch (next)
                {
                    case VisitorOverrideStatus.Closed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(VisitorOverrideStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"visitor override {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} ({Name})";
    }

    public double WeightedExposure()
    {
        return Math.Round((double)TenureMonths * (double)LeaveDays, 2);
    }

    public VisitorOverride Copy()
    {
        return new VisitorOverride(
            Code,
            Name,
            WindowDays,
            Margin,
            LeaveDays,
            ManagerId,
            Currency,
            TenureMonths,
            Retries,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
