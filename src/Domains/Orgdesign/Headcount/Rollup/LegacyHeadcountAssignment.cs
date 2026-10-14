namespace Acme.Workforce.Orgdesign.Headcount.Rollup;

using Acme.Workforce.Kit.Text;

/// <summary>Where a legacy headcount assignment is in its life cycle.</summary>
public enum LegacyHeadcountAssignmentStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>A legacy headcount assignment. Its code is unique within a tenant.</summary>
public sealed class LegacyHeadcountAssignment(string code, string title, int updatedAt, string country, int graceDays, LegacyHeadcountAssignmentStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int UpdatedAt { get; set; } = updatedAt;
    public string Country { get; set; } = country;
    public int GraceDays { get; set; } = graceDays;
    public LegacyHeadcountAssignmentStatus Status { get; set; } = status;

    /// <summary>Starts a legacy headcount assignment in status draft.</summary>
    public static LegacyHeadcountAssignment NewLegacyHeadcountAssignment(string code, string title)
    {
        return new LegacyHeadcountAssignment(code, title, 0, "", 0, LegacyHeadcountAssignmentStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LegacyHeadcountAssignmentStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LegacyHeadcountAssignmentStatus next)
    {
        switch (Status)
        {
            case LegacyHeadcountAssignmentStatus.Draft:
                return next == LegacyHeadcountAssignmentStatus.Closed;
            case LegacyHeadcountAssignmentStatus.Pending:
                switch (next)
                {
                    case LegacyHeadcountAssignmentStatus.Active:
                        return true;
                }
                return false;
            case LegacyHeadcountAssignmentStatus.Active:
                switch (next)
                {
                    case LegacyHeadcountAssignmentStatus.Closed:
                        return true;
                }
                return false;
            case LegacyHeadcountAssignmentStatus.Suspended:
                return next == LegacyHeadcountAssignmentStatus.Closed;
        }
        return false;
    }

    public void MoveTo(LegacyHeadcountAssignmentStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"legacy headcount assignment {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        UpdatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code}: {Title}";
    }

    public double NetVolume()
    {
        return Math.Round((double)UpdatedAt - (double)GraceDays, 2);
    }

    public LegacyHeadcountAssignment Copy()
    {
        return new LegacyHeadcountAssignment(Code, Title, UpdatedAt, Country, GraceDays, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Country);
    }
}
