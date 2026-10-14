namespace Acme.Workforce.Assets.Laptop.Bundle;

using Acme.Workforce.Kit.Text;

/// <summary>Where a laptop batch is in its life cycle.</summary>
public enum LaptopBatchStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>A laptop batch. Its code is unique within a tenant.</summary>
public sealed class LaptopBatch(string code, string name, string segment, int version, string channel, int graceDays, string country, string location, LaptopBatchStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Segment { get; set; } = segment;
    public int Version { get; set; } = version;
    public string Channel { get; set; } = channel;
    public int GraceDays { get; set; } = graceDays;
    public string Country { get; set; } = country;
    public string Location { get; set; } = location;
    public LaptopBatchStatus Status { get; set; } = status;

    /// <summary>Starts a laptop batch in status planned.</summary>
    public static LaptopBatch NewLaptopBatch(string code, string name)
    {
        return new LaptopBatch(code, name, "", 0, "", 0, "", "", LaptopBatchStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LaptopBatchStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LaptopBatchStatus next)
    {
        switch (Status)
        {
            case LaptopBatchStatus.Planned:
                return next == LaptopBatchStatus.Scheduled;
            case LaptopBatchStatus.Scheduled:
                return next == LaptopBatchStatus.Cancelled;
            case LaptopBatchStatus.InProgress:
                return next == LaptopBatchStatus.Cancelled;
            case LaptopBatchStatus.Completed:
                switch (next)
                {
                    case LaptopBatchStatus.Cancelled:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(LaptopBatchStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"laptop batch {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Version += 1;
    }

    public string Label()
    {
        return $"{"laptop batch"} {Code}";
    }

    public double ProjectedVolume()
    {
        return Math.Round((double)Version * (double)GraceDays, 2);
    }

    public LaptopBatch Copy()
    {
        return new LaptopBatch(Code, Name, Segment, Version, Channel, GraceDays, Country, Location, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Segment);
    }
}
