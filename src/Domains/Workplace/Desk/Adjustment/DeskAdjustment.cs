namespace Acme.Workforce.Workplace.Desk.Adjustment;

using Acme.Workforce.Kit.Text;

/// <summary>Where a desk adjustment is in its life cycle.</summary>
public enum DeskAdjustmentStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>The record of a desk adjustment and its current status.</summary>
public sealed class DeskAdjustment(string code, string title, string locale, string department, string salaryBand, string badge, bool approved, double amount, DeskAdjustmentStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Locale { get; set; } = locale;
    public string Department { get; set; } = department;
    public string SalaryBand { get; set; } = salaryBand;
    public string Badge { get; set; } = badge;
    public bool Approved { get; set; } = approved;
    public double Amount { get; set; } = amount;
    public DeskAdjustmentStatus Status { get; set; } = status;

    /// <summary>Starts a desk adjustment in status created.</summary>
    public static DeskAdjustment NewDeskAdjustment(string code, string title)
    {
        return new DeskAdjustment(code, title, "", "", "", "", false, 0.0, DeskAdjustmentStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case DeskAdjustmentStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(DeskAdjustmentStatus next)
    {
        switch (Status)
        {
            case DeskAdjustmentStatus.Created:
                return next == DeskAdjustmentStatus.Published;
            case DeskAdjustmentStatus.Validated:
                switch (next)
                {
                    case DeskAdjustmentStatus.Published:
                    case DeskAdjustmentStatus.Retired:
                        return true;
                }
                return false;
            case DeskAdjustmentStatus.Published:
                switch (next)
                {
                    case DeskAdjustmentStatus.Retired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(DeskAdjustmentStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"desk adjustment {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} / {Title}";
    }

    public double MeasuredValue()
    {
        return Amount;
    }

    public DeskAdjustment Copy()
    {
        return new DeskAdjustment(Code, Title, Locale, Department, SalaryBand, Badge, Approved, Amount, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Locale);
    }
}
