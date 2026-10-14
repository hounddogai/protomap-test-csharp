namespace Acme.Workforce.Orgdesign.Headcount.Schedule;

using Acme.Workforce.Kit.Text;

/// <summary>Where a headcount group is in its life cycle.</summary>
public enum HeadcountGroupStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>The record of a headcount group and its current status.</summary>
public sealed class HeadcountGroup(string code, string name, double floorValue, string grade, int expiresAt, double amount, string reviewer, int endsAt, int priority, string location, HeadcountGroupStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public double FloorValue { get; set; } = floorValue;
    public string Grade { get; set; } = grade;
    public int ExpiresAt { get; set; } = expiresAt;
    public double Amount { get; set; } = amount;
    public string Reviewer { get; set; } = reviewer;
    public int EndsAt { get; set; } = endsAt;
    public int Priority { get; set; } = priority;
    public string Location { get; set; } = location;
    public HeadcountGroupStatus Status { get; set; } = status;

    /// <summary>Starts a headcount group in status idle.</summary>
    public static HeadcountGroup NewHeadcountGroup(string code, string name)
    {
        return new HeadcountGroup(code, name, 0.0, "", 0, 0.0, "", 0, 0, "", HeadcountGroupStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case HeadcountGroupStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(HeadcountGroupStatus next)
    {
        switch (Status)
        {
            case HeadcountGroupStatus.Idle:
                switch (next)
                {
                    case HeadcountGroupStatus.Warming:
                        return true;
                }
                return false;
            case HeadcountGroupStatus.Warming:
                switch (next)
                {
                    case HeadcountGroupStatus.Stopped:
                        return true;
                }
                return false;
            case HeadcountGroupStatus.Ready:
                return next == HeadcountGroupStatus.Stopped;
            case HeadcountGroupStatus.Draining:
                switch (next)
                {
                    case HeadcountGroupStatus.Stopped:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(HeadcountGroupStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"headcount group {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        ExpiresAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code}: {Name}";
    }

    public double NetLoad()
    {
        return Math.Round((double)Priority * FloorValue, 2);
    }

    public HeadcountGroup Copy()
    {
        return new HeadcountGroup(
            Code,
            Name,
            FloorValue,
            Grade,
            ExpiresAt,
            Amount,
            Reviewer,
            EndsAt,
            Priority,
            Location,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
