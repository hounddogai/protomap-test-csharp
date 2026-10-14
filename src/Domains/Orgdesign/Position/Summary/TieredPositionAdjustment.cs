namespace Acme.Workforce.Orgdesign.Position.Summary;

using Acme.Workforce.Kit.Text;

/// <summary>Where a tiered position adjustment is in its life cycle.</summary>
public enum TieredPositionAdjustmentStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>One tiered position adjustment, with what the planners need to act on it.</summary>
public sealed class TieredPositionAdjustment(string code, string name, int windowDays, int tenureMonths, string employeeId, int startsAt, string contactEmail, string grade, string category, TieredPositionAdjustmentStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int WindowDays { get; set; } = windowDays;
    public int TenureMonths { get; set; } = tenureMonths;
    public string EmployeeId { get; set; } = employeeId;
    public int StartsAt { get; set; } = startsAt;
    public string ContactEmail { get; set; } = contactEmail;
    public string Grade { get; set; } = grade;
    public string Category { get; set; } = category;
    public TieredPositionAdjustmentStatus Status { get; set; } = status;

    /// <summary>Starts a tiered position adjustment in status draft.</summary>
    public static TieredPositionAdjustment NewTieredPositionAdjustment(string code, string name)
    {
        return new TieredPositionAdjustment(
            code,
            name,
            0,
            0,
            "",
            0,
            "",
            "",
            "",
            TieredPositionAdjustmentStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case TieredPositionAdjustmentStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(TieredPositionAdjustmentStatus next)
    {
        switch (Status)
        {
            case TieredPositionAdjustmentStatus.Draft:
                return next == TieredPositionAdjustmentStatus.Pending || next == TieredPositionAdjustmentStatus.Active;
            case TieredPositionAdjustmentStatus.Pending:
                switch (next)
                {
                    case TieredPositionAdjustmentStatus.Suspended:
                        return true;
                }
                return false;
            case TieredPositionAdjustmentStatus.Active:
                return next == TieredPositionAdjustmentStatus.Suspended;
            case TieredPositionAdjustmentStatus.Suspended:
                return next == TieredPositionAdjustmentStatus.Closed;
        }
        return false;
    }

    public void MoveTo(TieredPositionAdjustmentStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"tiered position adjustment {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        StartsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} / {Name}";
    }

    public double ProjectedValue()
    {
        return Math.Round((double)TenureMonths + (double)StartsAt, 2);
    }

    public TieredPositionAdjustment Copy()
    {
        return new TieredPositionAdjustment(
            Code,
            Name,
            WindowDays,
            TenureMonths,
            EmployeeId,
            StartsAt,
            ContactEmail,
            Grade,
            Category,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Category);
    }
}
