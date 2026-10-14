namespace Acme.Workforce.Talent.CareerPath.Metric;

using Acme.Workforce.Kit.Text;

/// <summary>Where a career path entry is in its life cycle.</summary>
public enum CareerPathEntryStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>The record of a career path entry and its current status.</summary>
public sealed class CareerPathEntry(string code, string name, string managerId, string title, int leaveDays, string category, bool active, string ownerId, List<string> tags, CareerPathEntryStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string ManagerId { get; set; } = managerId;
    public string Title { get; set; } = title;
    public int LeaveDays { get; set; } = leaveDays;
    public string Category { get; set; } = category;
    public bool Active { get; set; } = active;
    public string OwnerId { get; set; } = ownerId;
    public List<string> Tags { get; set; } = tags;
    public CareerPathEntryStatus Status { get; set; } = status;

    /// <summary>Starts a career path entry in status requested.</summary>
    public static CareerPathEntry NewCareerPathEntry(string code, string name)
    {
        return new CareerPathEntry(
            code,
            name,
            "",
            "",
            0,
            "",
            false,
            "",
            new List<string>(),
            CareerPathEntryStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case CareerPathEntryStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(CareerPathEntryStatus next)
    {
        switch (Status)
        {
            case CareerPathEntryStatus.Requested:
                return next == CareerPathEntryStatus.Granted || next == CareerPathEntryStatus.Expired;
            case CareerPathEntryStatus.Granted:
                return next == CareerPathEntryStatus.Revoked || next == CareerPathEntryStatus.Expired;
            case CareerPathEntryStatus.Revoked:
                return next == CareerPathEntryStatus.Expired;
        }
        return false;
    }

    public void MoveTo(CareerPathEntryStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"career path entry {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"career path entry"} {Code}";
    }

    public bool HasTag(string tag)
    {
        foreach (var candidate in Tags)
        {
            if (Text.Normalize(candidate) == Text.Normalize(tag))
            {
                return true;
            }
        }
        return false;
    }

    public double MeasuredValue()
    {
        return (double)LeaveDays;
    }

    public CareerPathEntry Copy()
    {
        return new CareerPathEntry(Code, Name, ManagerId, Title, LeaveDays, Category, Active, OwnerId, Tags, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Category);
    }
}
