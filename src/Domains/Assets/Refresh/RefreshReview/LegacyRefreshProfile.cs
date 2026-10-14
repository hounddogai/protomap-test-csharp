namespace Acme.Workforce.Assets.Refresh.RefreshReview;

using Acme.Workforce.Kit.Text;

/// <summary>Where a legacy refresh profile is in its life cycle.</summary>
public enum LegacyRefreshProfileStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>A legacy refresh profile as the refresh team keeps it.</summary>
public sealed class LegacyRefreshProfile(string code, string label, string costCenter, int windowDays, string employeeId, string phone, string description, int limit, int tenureMonths, int attempts, string team, LegacyRefreshProfileStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string CostCenter { get; set; } = costCenter;
    public int WindowDays { get; set; } = windowDays;
    public string EmployeeId { get; set; } = employeeId;
    public string Phone { get; set; } = phone;
    public string Description { get; set; } = description;
    public int Limit { get; set; } = limit;
    public int TenureMonths { get; set; } = tenureMonths;
    public int Attempts { get; set; } = attempts;
    public string Team { get; set; } = team;
    public LegacyRefreshProfileStatus Status { get; set; } = status;

    /// <summary>Starts a legacy refresh profile in status planned.</summary>
    public static LegacyRefreshProfile NewLegacyRefreshProfile(string code, string label)
    {
        return new LegacyRefreshProfile(
            code,
            label,
            "",
            0,
            "",
            "",
            "",
            0,
            0,
            0,
            "",
            LegacyRefreshProfileStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LegacyRefreshProfileStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LegacyRefreshProfileStatus next)
    {
        switch (Status)
        {
            case LegacyRefreshProfileStatus.Planned:
                switch (next)
                {
                    case LegacyRefreshProfileStatus.InProgress:
                        return true;
                }
                return false;
            case LegacyRefreshProfileStatus.Scheduled:
                switch (next)
                {
                    case LegacyRefreshProfileStatus.InProgress:
                    case LegacyRefreshProfileStatus.Cancelled:
                        return true;
                }
                return false;
            case LegacyRefreshProfileStatus.InProgress:
                return next == LegacyRefreshProfileStatus.Cancelled;
            case LegacyRefreshProfileStatus.Completed:
                switch (next)
                {
                    case LegacyRefreshProfileStatus.Cancelled:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(LegacyRefreshProfileStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"legacy refresh profile {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code}: {Label}";
    }

    public double WeightedLoad()
    {
        return Math.Round((double)Limit - (double)WindowDays, 2);
    }

    public LegacyRefreshProfile Copy()
    {
        return new LegacyRefreshProfile(
            Code,
            Label,
            CostCenter,
            WindowDays,
            EmployeeId,
            Phone,
            Description,
            Limit,
            TenureMonths,
            Attempts,
            Team,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Team);
    }
}
