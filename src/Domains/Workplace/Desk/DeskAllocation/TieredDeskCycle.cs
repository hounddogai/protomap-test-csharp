namespace Acme.Workforce.Workplace.Desk.DeskAllocation;

using Acme.Workforce.Kit.Text;

/// <summary>Where a tiered desk cycle is in its life cycle.</summary>
public enum TieredDeskCycleStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>One tiered desk cycle, with what the planners need to act on it.</summary>
public sealed class TieredDeskCycle(string code, string label, string description, string contactEmail, string ownerId, string phone, int retries, string locale, string timezone, List<string> tags, TieredDeskCycleStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Description { get; set; } = description;
    public string ContactEmail { get; set; } = contactEmail;
    public string OwnerId { get; set; } = ownerId;
    public string Phone { get; set; } = phone;
    public int Retries { get; set; } = retries;
    public string Locale { get; set; } = locale;
    public string Timezone { get; set; } = timezone;
    public List<string> Tags { get; set; } = tags;
    public TieredDeskCycleStatus Status { get; set; } = status;

    /// <summary>Starts a tiered desk cycle in status created.</summary>
    public static TieredDeskCycle NewTieredDeskCycle(string code, string label)
    {
        return new TieredDeskCycle(
            code,
            label,
            "",
            "",
            "",
            "",
            0,
            "",
            "",
            new List<string>(),
            TieredDeskCycleStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case TieredDeskCycleStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(TieredDeskCycleStatus next)
    {
        switch (Status)
        {
            case TieredDeskCycleStatus.Created:
                return next == TieredDeskCycleStatus.Retired;
            case TieredDeskCycleStatus.Validated:
                switch (next)
                {
                    case TieredDeskCycleStatus.Published:
                        return true;
                }
                return false;
            case TieredDeskCycleStatus.Published:
                return next == TieredDeskCycleStatus.Retired;
        }
        return false;
    }

    public void MoveTo(TieredDeskCycleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"tiered desk cycle {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} ({Label})";
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
        return (double)Retries;
    }

    public TieredDeskCycle Copy()
    {
        return new TieredDeskCycle(
            Code,
            Label,
            Description,
            ContactEmail,
            OwnerId,
            Phone,
            Retries,
            Locale,
            Timezone,
            Tags,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Locale);
    }
}
