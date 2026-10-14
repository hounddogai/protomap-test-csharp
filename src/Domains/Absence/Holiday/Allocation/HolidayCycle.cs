namespace Acme.Workforce.Absence.Holiday.Allocation;

using Acme.Workforce.Kit.Text;

/// <summary>Where a holiday cycle is in its life cycle.</summary>
public enum HolidayCycleStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>A holiday cycle as the holiday team keeps it.</summary>
public sealed class HolidayCycle(string code, string title, bool approved, string checksum, int batchSize, HolidayCycleStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public bool Approved { get; set; } = approved;
    public string Checksum { get; set; } = checksum;
    public int BatchSize { get; set; } = batchSize;
    public HolidayCycleStatus Status { get; set; } = status;

    /// <summary>Starts a holiday cycle in status created.</summary>
    public static HolidayCycle NewHolidayCycle(string code, string title)
    {
        return new HolidayCycle(code, title, false, "", 0, HolidayCycleStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case HolidayCycleStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(HolidayCycleStatus next)
    {
        switch (Status)
        {
            case HolidayCycleStatus.Created:
                switch (next)
                {
                    case HolidayCycleStatus.Validated:
                    case HolidayCycleStatus.Published:
                        return true;
                }
                return false;
            case HolidayCycleStatus.Validated:
                switch (next)
                {
                    case HolidayCycleStatus.Published:
                    case HolidayCycleStatus.Retired:
                        return true;
                }
                return false;
            case HolidayCycleStatus.Published:
                return next == HolidayCycleStatus.Retired;
        }
        return false;
    }

    public void MoveTo(HolidayCycleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"holiday cycle {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"holiday cycle"} {Code}";
    }

    public double MeasuredValue()
    {
        return (double)BatchSize;
    }

    public HolidayCycle Copy()
    {
        return new HolidayCycle(Code, Title, Approved, Checksum, BatchSize, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
