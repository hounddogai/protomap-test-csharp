namespace Acme.Workforce.Assets.Laptop.Snapshot;

using Acme.Workforce.Kit.Text;

/// <summary>Where a default laptop segment is in its life cycle.</summary>
public enum DefaultLaptopSegmentStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>A default laptop segment as the laptop team keeps it.</summary>
public sealed class DefaultLaptopSegment(string code, string label, int quantity, string salaryBand, string summary, double margin, DefaultLaptopSegmentStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int Quantity { get; set; } = quantity;
    public string SalaryBand { get; set; } = salaryBand;
    public string Summary { get; set; } = summary;
    public double Margin { get; set; } = margin;
    public DefaultLaptopSegmentStatus Status { get; set; } = status;

    /// <summary>Starts a default laptop segment in status planned.</summary>
    public static DefaultLaptopSegment NewDefaultLaptopSegment(string code, string label)
    {
        return new DefaultLaptopSegment(code, label, 0, "", "", 0.0, DefaultLaptopSegmentStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case DefaultLaptopSegmentStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(DefaultLaptopSegmentStatus next)
    {
        switch (Status)
        {
            case DefaultLaptopSegmentStatus.Planned:
                return next == DefaultLaptopSegmentStatus.Scheduled;
            case DefaultLaptopSegmentStatus.Scheduled:
                return next == DefaultLaptopSegmentStatus.Completed || next == DefaultLaptopSegmentStatus.Cancelled;
            case DefaultLaptopSegmentStatus.InProgress:
                return next == DefaultLaptopSegmentStatus.Cancelled;
            case DefaultLaptopSegmentStatus.Completed:
                return next == DefaultLaptopSegmentStatus.Cancelled;
        }
        return false;
    }

    public void MoveTo(DefaultLaptopSegmentStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"default laptop segment {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} ({Label})";
    }

    public double NetVolume()
    {
        return Math.Round(Margin + (double)Quantity, 2);
    }

    public DefaultLaptopSegment Copy()
    {
        return new DefaultLaptopSegment(Code, Label, Quantity, SalaryBand, Summary, Margin, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
