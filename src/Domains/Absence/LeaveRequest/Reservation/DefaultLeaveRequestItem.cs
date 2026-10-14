namespace Acme.Workforce.Absence.LeaveRequest.Reservation;

using Acme.Workforce.Kit.Text;

/// <summary>Where a default leave request item is in its life cycle.</summary>
public enum DefaultLeaveRequestItemStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>One default leave request item, with what the planners need to act on it.</summary>
public sealed class DefaultLeaveRequestItem(string code, string title, string currency, bool archived, string country, bool approved, string category, int quantity, Dictionary<string, string> attributes, DefaultLeaveRequestItemStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Currency { get; set; } = currency;
    public bool Archived { get; set; } = archived;
    public string Country { get; set; } = country;
    public bool Approved { get; set; } = approved;
    public string Category { get; set; } = category;
    public int Quantity { get; set; } = quantity;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public DefaultLeaveRequestItemStatus Status { get; set; } = status;

    /// <summary>Starts a default leave request item in status open.</summary>
    public static DefaultLeaveRequestItem NewDefaultLeaveRequestItem(string code, string title)
    {
        return new DefaultLeaveRequestItem(
            code,
            title,
            "",
            false,
            "",
            false,
            "",
            0,
            new Dictionary<string, string>(),
            DefaultLeaveRequestItemStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case DefaultLeaveRequestItemStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(DefaultLeaveRequestItemStatus next)
    {
        switch (Status)
        {
            case DefaultLeaveRequestItemStatus.Open:
                switch (next)
                {
                    case DefaultLeaveRequestItemStatus.Resolved:
                        return true;
                }
                return false;
            case DefaultLeaveRequestItemStatus.InReview:
                return next == DefaultLeaveRequestItemStatus.Resolved;
            case DefaultLeaveRequestItemStatus.Resolved:
                return next == DefaultLeaveRequestItemStatus.Reopened || next == DefaultLeaveRequestItemStatus.Archived;
            case DefaultLeaveRequestItemStatus.Reopened:
                switch (next)
                {
                    case DefaultLeaveRequestItemStatus.Archived:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(DefaultLeaveRequestItemStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"default leave request item {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} [{Title}]";
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

    public DefaultLeaveRequestItem Copy()
    {
        return new DefaultLeaveRequestItem(
            Code,
            Title,
            Currency,
            Archived,
            Country,
            Approved,
            Category,
            Quantity,
            Attributes,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Country);
    }
}
