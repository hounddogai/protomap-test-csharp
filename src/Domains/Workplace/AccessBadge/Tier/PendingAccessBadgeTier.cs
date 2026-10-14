namespace Acme.Workforce.Workplace.AccessBadge.Tier;

using Acme.Workforce.Kit.Text;

/// <summary>Where a pending access badge tier is in its life cycle.</summary>
public enum PendingAccessBadgeTierStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>A pending access badge tier. Its code is unique within a tenant.</summary>
public sealed class PendingAccessBadgeTier(string code, string label, string phone, List<string> tags, string title, string department, int durationMinutes, bool visible, int updatedAt, string summary, PendingAccessBadgeTierStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Phone { get; set; } = phone;
    public List<string> Tags { get; set; } = tags;
    public string Title { get; set; } = title;
    public string Department { get; set; } = department;
    public int DurationMinutes { get; set; } = durationMinutes;
    public bool Visible { get; set; } = visible;
    public int UpdatedAt { get; set; } = updatedAt;
    public string Summary { get; set; } = summary;
    public PendingAccessBadgeTierStatus Status { get; set; } = status;

    /// <summary>Starts a pending access badge tier in status draft.</summary>
    public static PendingAccessBadgeTier NewPendingAccessBadgeTier(string code, string label)
    {
        return new PendingAccessBadgeTier(
            code,
            label,
            "",
            new List<string>(),
            "",
            "",
            0,
            false,
            0,
            "",
            PendingAccessBadgeTierStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PendingAccessBadgeTierStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PendingAccessBadgeTierStatus next)
    {
        switch (Status)
        {
            case PendingAccessBadgeTierStatus.Draft:
                switch (next)
                {
                    case PendingAccessBadgeTierStatus.Closed:
                        return true;
                }
                return false;
            case PendingAccessBadgeTierStatus.Pending:
                return next == PendingAccessBadgeTierStatus.Suspended || next == PendingAccessBadgeTierStatus.Closed;
            case PendingAccessBadgeTierStatus.Active:
                switch (next)
                {
                    case PendingAccessBadgeTierStatus.Suspended:
                    case PendingAccessBadgeTierStatus.Closed:
                        return true;
                }
                return false;
            case PendingAccessBadgeTierStatus.Suspended:
                switch (next)
                {
                    case PendingAccessBadgeTierStatus.Closed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(PendingAccessBadgeTierStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"pending access badge tier {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        UpdatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{Code}: {Label}";
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

    public double NetLoad()
    {
        return Math.Round((double)UpdatedAt + (double)DurationMinutes, 2);
    }

    public PendingAccessBadgeTier Copy()
    {
        return new PendingAccessBadgeTier(
            Code,
            Label,
            Phone,
            Tags,
            Title,
            Department,
            DurationMinutes,
            Visible,
            UpdatedAt,
            Summary,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Department);
    }
}
