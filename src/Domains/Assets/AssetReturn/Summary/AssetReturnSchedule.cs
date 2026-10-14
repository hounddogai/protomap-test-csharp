namespace Acme.Workforce.Assets.AssetReturn.Summary;

using Acme.Workforce.Kit.Text;

/// <summary>Where a asset return schedule is in its life cycle.</summary>
public enum AssetReturnScheduleStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>The record of a asset return schedule and its current status.</summary>
public sealed class AssetReturnSchedule(string code, string title, int tenureMonths, string employeeId, int revision, AssetReturnScheduleStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int TenureMonths { get; set; } = tenureMonths;
    public string EmployeeId { get; set; } = employeeId;
    public int Revision { get; set; } = revision;
    public AssetReturnScheduleStatus Status { get; set; } = status;

    /// <summary>Starts a asset return schedule in status planned.</summary>
    public static AssetReturnSchedule NewAssetReturnSchedule(string code, string title)
    {
        return new AssetReturnSchedule(code, title, 0, "", 0, AssetReturnScheduleStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AssetReturnScheduleStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AssetReturnScheduleStatus next)
    {
        switch (Status)
        {
            case AssetReturnScheduleStatus.Planned:
                return next == AssetReturnScheduleStatus.Scheduled;
            case AssetReturnScheduleStatus.Scheduled:
                switch (next)
                {
                    case AssetReturnScheduleStatus.InProgress:
                    case AssetReturnScheduleStatus.Cancelled:
                        return true;
                }
                return false;
            case AssetReturnScheduleStatus.InProgress:
                switch (next)
                {
                    case AssetReturnScheduleStatus.Cancelled:
                        return true;
                }
                return false;
            case AssetReturnScheduleStatus.Completed:
                switch (next)
                {
                    case AssetReturnScheduleStatus.Cancelled:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(AssetReturnScheduleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"asset return schedule {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Revision += 1;
    }

    public string Label()
    {
        return $"{Code}: {Title}";
    }

    public double AdjustedTotal()
    {
        return Math.Round((double)Revision + (double)TenureMonths, 2);
    }

    public AssetReturnSchedule Copy()
    {
        return new AssetReturnSchedule(Code, Title, TenureMonths, EmployeeId, Revision, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
