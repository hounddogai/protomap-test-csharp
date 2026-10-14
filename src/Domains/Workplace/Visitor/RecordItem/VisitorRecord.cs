namespace Acme.Workforce.Workplace.Visitor.RecordItem;

using Acme.Workforce.Kit.Text;

/// <summary>Where a visitor record is in its life cycle.</summary>
public enum VisitorRecordStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>One visitor record, with what the planners need to act on it.</summary>
public sealed class VisitorRecord(string code, string title, int createdAt, string badge, string region, double weight, string phone, VisitorRecordStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int CreatedAt { get; set; } = createdAt;
    public string Badge { get; set; } = badge;
    public string Region { get; set; } = region;
    public double Weight { get; set; } = weight;
    public string Phone { get; set; } = phone;
    public VisitorRecordStatus Status { get; set; } = status;

    /// <summary>Starts a visitor record in status planned.</summary>
    public static VisitorRecord NewVisitorRecord(string code, string title)
    {
        return new VisitorRecord(code, title, 0, "", "", 0.0, "", VisitorRecordStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case VisitorRecordStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(VisitorRecordStatus next)
    {
        switch (Status)
        {
            case VisitorRecordStatus.Planned:
                switch (next)
                {
                    case VisitorRecordStatus.InProgress:
                        return true;
                }
                return false;
            case VisitorRecordStatus.Scheduled:
                switch (next)
                {
                    case VisitorRecordStatus.InProgress:
                    case VisitorRecordStatus.Completed:
                        return true;
                }
                return false;
            case VisitorRecordStatus.InProgress:
                return next == VisitorRecordStatus.Completed || next == VisitorRecordStatus.Cancelled;
            case VisitorRecordStatus.Completed:
                switch (next)
                {
                    case VisitorRecordStatus.Cancelled:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(VisitorRecordStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"visitor record {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        CreatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} [{Title}]";
    }

    public double WeightedValue()
    {
        return Math.Round((double)CreatedAt - Weight, 2);
    }

    public VisitorRecord Copy()
    {
        return new VisitorRecord(Code, Title, CreatedAt, Badge, Region, Weight, Phone, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Region);
    }
}
