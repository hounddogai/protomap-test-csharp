namespace Acme.Workforce.Assets.Warranty.Forecast;

using Acme.Workforce.Kit.Text;

/// <summary>Where a warranty segment is in its life cycle.</summary>
public enum WarrantySegmentStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>The record of a warranty segment and its current status.</summary>
public sealed class WarrantySegment(string code, string label, string name, int capacity, int startsAt, string channel, WarrantySegmentStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Name { get; set; } = name;
    public int Capacity { get; set; } = capacity;
    public int StartsAt { get; set; } = startsAt;
    public string Channel { get; set; } = channel;
    public WarrantySegmentStatus Status { get; set; } = status;

    /// <summary>Starts a warranty segment in status idle.</summary>
    public static WarrantySegment NewWarrantySegment(string code, string label)
    {
        return new WarrantySegment(code, label, "", 0, 0, "", WarrantySegmentStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case WarrantySegmentStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(WarrantySegmentStatus next)
    {
        switch (Status)
        {
            case WarrantySegmentStatus.Idle:
                return next == WarrantySegmentStatus.Draining || next == WarrantySegmentStatus.Stopped;
            case WarrantySegmentStatus.Warming:
                return next == WarrantySegmentStatus.Ready || next == WarrantySegmentStatus.Draining;
            case WarrantySegmentStatus.Ready:
                return next == WarrantySegmentStatus.Draining;
            case WarrantySegmentStatus.Draining:
                switch (next)
                {
                    case WarrantySegmentStatus.Stopped:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(WarrantySegmentStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"warranty segment {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        StartsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{"warranty segment"} {Code}";
    }

    public double NetTotal()
    {
        return Math.Round((double)Capacity + (double)StartsAt, 2);
    }

    public WarrantySegment Copy()
    {
        return new WarrantySegment(Code, Label, Name, Capacity, StartsAt, Channel, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Channel);
    }
}
