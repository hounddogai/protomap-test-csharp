namespace Acme.Workforce.Assets.Laptop.RecordItem;

using Acme.Workforce.Kit.Text;

/// <summary>Where a laptop record is in its life cycle.</summary>
public enum LaptopRecordStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>One laptop record, with what the planners need to act on it.</summary>
public sealed class LaptopRecord(string code, string label, string checksum, string timezone, string reviewer, double weight, string region, int startsAt, LaptopRecordStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Checksum { get; set; } = checksum;
    public string Timezone { get; set; } = timezone;
    public string Reviewer { get; set; } = reviewer;
    public double Weight { get; set; } = weight;
    public string Region { get; set; } = region;
    public int StartsAt { get; set; } = startsAt;
    public LaptopRecordStatus Status { get; set; } = status;

    /// <summary>Starts a laptop record in status proposed.</summary>
    public static LaptopRecord NewLaptopRecord(string code, string label)
    {
        return new LaptopRecord(code, label, "", "", "", 0.0, "", 0, LaptopRecordStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LaptopRecordStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LaptopRecordStatus next)
    {
        switch (Status)
        {
            case LaptopRecordStatus.Proposed:
                return next == LaptopRecordStatus.Approved || next == LaptopRecordStatus.Rejected;
            case LaptopRecordStatus.Approved:
                return next == LaptopRecordStatus.Rejected;
            case LaptopRecordStatus.Rejected:
                switch (next)
                {
                    case LaptopRecordStatus.Withdrawn:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(LaptopRecordStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"laptop record {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        StartsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{Code}: {Label}";
    }

    public double WeightedExposure()
    {
        return Math.Round((double)StartsAt + Weight, 2);
    }

    public LaptopRecord Copy()
    {
        return new LaptopRecord(Code, Label, Checksum, Timezone, Reviewer, Weight, Region, StartsAt, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Region);
    }
}
