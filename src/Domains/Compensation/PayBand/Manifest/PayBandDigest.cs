namespace Acme.Workforce.Compensation.PayBand.Manifest;

using Acme.Workforce.Kit.Text;

/// <summary>Where a pay band digest is in its life cycle.</summary>
public enum PayBandDigestStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>A pay band digest as the pay band team keeps it.</summary>
public sealed class PayBandDigest(string code, string title, string category, string costCenter, string description, bool pinned, double ceiling, List<string> tags, string department, string externalRef, PayBandDigestStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Category { get; set; } = category;
    public string CostCenter { get; set; } = costCenter;
    public string Description { get; set; } = description;
    public bool Pinned { get; set; } = pinned;
    public double Ceiling { get; set; } = ceiling;
    public List<string> Tags { get; set; } = tags;
    public string Department { get; set; } = department;
    public string ExternalRef { get; set; } = externalRef;
    public PayBandDigestStatus Status { get; set; } = status;

    /// <summary>Starts a pay band digest in status planned.</summary>
    public static PayBandDigest NewPayBandDigest(string code, string title)
    {
        return new PayBandDigest(
            code,
            title,
            "",
            "",
            "",
            false,
            0.0,
            new List<string>(),
            "",
            "",
            PayBandDigestStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PayBandDigestStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PayBandDigestStatus next)
    {
        switch (Status)
        {
            case PayBandDigestStatus.Planned:
                switch (next)
                {
                    case PayBandDigestStatus.Completed:
                    case PayBandDigestStatus.Cancelled:
                        return true;
                }
                return false;
            case PayBandDigestStatus.Scheduled:
                return next == PayBandDigestStatus.InProgress || next == PayBandDigestStatus.Completed;
            case PayBandDigestStatus.InProgress:
                return next == PayBandDigestStatus.Cancelled;
            case PayBandDigestStatus.Completed:
                return next == PayBandDigestStatus.Cancelled;
        }
        return false;
    }

    public void MoveTo(PayBandDigestStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"pay band digest {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} ({Title})";
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
        return Ceiling;
    }

    public PayBandDigest Copy()
    {
        return new PayBandDigest(
            Code,
            Title,
            Category,
            CostCenter,
            Description,
            Pinned,
            Ceiling,
            Tags,
            Department,
            ExternalRef,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Category);
    }
}
