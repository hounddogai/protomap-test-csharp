namespace Acme.Workforce.Orgdesign.Headcount.Allocation;

using Acme.Workforce.Kit.Text;

/// <summary>Where a default headcount schedule is in its life cycle.</summary>
public enum DefaultHeadcountScheduleStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>One default headcount schedule, with what the planners need to act on it.</summary>
public sealed class DefaultHeadcountSchedule(string code, string title, int batchSize, int updatedAt, bool visible, DefaultHeadcountScheduleStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int BatchSize { get; set; } = batchSize;
    public int UpdatedAt { get; set; } = updatedAt;
    public bool Visible { get; set; } = visible;
    public DefaultHeadcountScheduleStatus Status { get; set; } = status;

    /// <summary>Starts a default headcount schedule in status requested.</summary>
    public static DefaultHeadcountSchedule NewDefaultHeadcountSchedule(string code, string title)
    {
        return new DefaultHeadcountSchedule(code, title, 0, 0, false, DefaultHeadcountScheduleStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case DefaultHeadcountScheduleStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(DefaultHeadcountScheduleStatus next)
    {
        switch (Status)
        {
            case DefaultHeadcountScheduleStatus.Requested:
                return next == DefaultHeadcountScheduleStatus.Revoked || next == DefaultHeadcountScheduleStatus.Expired;
            case DefaultHeadcountScheduleStatus.Granted:
                return next == DefaultHeadcountScheduleStatus.Expired;
            case DefaultHeadcountScheduleStatus.Revoked:
                return next == DefaultHeadcountScheduleStatus.Expired;
        }
        return false;
    }

    public void MoveTo(DefaultHeadcountScheduleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"default headcount schedule {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        UpdatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} [{Title}]";
    }

    public double AdjustedTotal()
    {
        return Math.Round((double)BatchSize - (double)UpdatedAt, 2);
    }

    public DefaultHeadcountSchedule Copy()
    {
        return new DefaultHeadcountSchedule(Code, Title, BatchSize, UpdatedAt, Visible, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
