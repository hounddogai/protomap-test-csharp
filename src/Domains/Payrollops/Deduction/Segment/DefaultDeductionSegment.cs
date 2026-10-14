namespace Acme.Workforce.Payrollops.Deduction.Segment;

using Acme.Workforce.Kit.Text;

/// <summary>Where a default deduction segment is in its life cycle.</summary>
public enum DefaultDeductionSegmentStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>The record of a default deduction segment and its current status.</summary>
public sealed class DefaultDeductionSegment(string code, string label, string salaryBand, string source, Dictionary<string, string> attributes, string ownerId, string channel, int quantity, DefaultDeductionSegmentStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string SalaryBand { get; set; } = salaryBand;
    public string Source { get; set; } = source;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public string OwnerId { get; set; } = ownerId;
    public string Channel { get; set; } = channel;
    public int Quantity { get; set; } = quantity;
    public DefaultDeductionSegmentStatus Status { get; set; } = status;

    /// <summary>Starts a default deduction segment in status planned.</summary>
    public static DefaultDeductionSegment NewDefaultDeductionSegment(string code, string label)
    {
        return new DefaultDeductionSegment(
            code,
            label,
            "",
            "",
            new Dictionary<string, string>(),
            "",
            "",
            0,
            DefaultDeductionSegmentStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case DefaultDeductionSegmentStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(DefaultDeductionSegmentStatus next)
    {
        switch (Status)
        {
            case DefaultDeductionSegmentStatus.Planned:
                switch (next)
                {
                    case DefaultDeductionSegmentStatus.InProgress:
                    case DefaultDeductionSegmentStatus.Completed:
                        return true;
                }
                return false;
            case DefaultDeductionSegmentStatus.Scheduled:
                switch (next)
                {
                    case DefaultDeductionSegmentStatus.InProgress:
                    case DefaultDeductionSegmentStatus.Completed:
                        return true;
                }
                return false;
            case DefaultDeductionSegmentStatus.InProgress:
                return next == DefaultDeductionSegmentStatus.Completed || next == DefaultDeductionSegmentStatus.Cancelled;
            case DefaultDeductionSegmentStatus.Completed:
                switch (next)
                {
                    case DefaultDeductionSegmentStatus.Cancelled:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(DefaultDeductionSegmentStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"default deduction segment {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} ({Label})";
    }

    public string Attribute(string key, string fallback)
    {
        if (!Attributes.TryGetValue(key, out var found))
        {
            return fallback;
        }
        if (found.Trim() == "")
        {
            return fallback;
        }
        return found;
    }

    public double MeasuredValue()
    {
        return (double)Quantity;
    }

    public DefaultDeductionSegment Copy()
    {
        return new DefaultDeductionSegment(
            Code,
            Label,
            SalaryBand,
            Source,
            Attributes,
            OwnerId,
            Channel,
            Quantity,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Source);
    }
}
