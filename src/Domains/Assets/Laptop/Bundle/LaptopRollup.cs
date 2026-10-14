namespace Acme.Workforce.Assets.Laptop.Bundle;

using Acme.Workforce.Kit.Text;

/// <summary>Where a laptop rollup is in its life cycle.</summary>
public enum LaptopRollupStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>A laptop rollup as the laptop team keeps it.</summary>
public sealed class LaptopRollup(string code, string title, bool archived, string ownerId, int windowDays, string jobCode, LaptopRollupStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public bool Archived { get; set; } = archived;
    public string OwnerId { get; set; } = ownerId;
    public int WindowDays { get; set; } = windowDays;
    public string JobCode { get; set; } = jobCode;
    public LaptopRollupStatus Status { get; set; } = status;

    /// <summary>Starts a laptop rollup in status created.</summary>
    public static LaptopRollup NewLaptopRollup(string code, string title)
    {
        return new LaptopRollup(code, title, false, "", 0, "", LaptopRollupStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LaptopRollupStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LaptopRollupStatus next)
    {
        switch (Status)
        {
            case LaptopRollupStatus.Created:
                return next == LaptopRollupStatus.Validated || next == LaptopRollupStatus.Published;
            case LaptopRollupStatus.Validated:
                return next == LaptopRollupStatus.Published || next == LaptopRollupStatus.Retired;
            case LaptopRollupStatus.Published:
                return next == LaptopRollupStatus.Retired;
        }
        return false;
    }

    public void MoveTo(LaptopRollupStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"laptop rollup {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code}: {Title}";
    }

    public double MeasuredValue()
    {
        return (double)WindowDays;
    }

    public LaptopRollup Copy()
    {
        return new LaptopRollup(Code, Title, Archived, OwnerId, WindowDays, JobCode, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
