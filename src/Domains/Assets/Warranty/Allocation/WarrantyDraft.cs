namespace Acme.Workforce.Assets.Warranty.Allocation;

using Acme.Workforce.Kit.Text;

/// <summary>Where a warranty draft is in its life cycle.</summary>
public enum WarrantyDraftStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>One warranty draft, with what the planners need to act on it.</summary>
public sealed class WarrantyDraft(string code, string title, List<string> tags, string currency, double floorValue, int startsAt, string ownerId, int attempts, int tenureMonths, WarrantyDraftStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public List<string> Tags { get; set; } = tags;
    public string Currency { get; set; } = currency;
    public double FloorValue { get; set; } = floorValue;
    public int StartsAt { get; set; } = startsAt;
    public string OwnerId { get; set; } = ownerId;
    public int Attempts { get; set; } = attempts;
    public int TenureMonths { get; set; } = tenureMonths;
    public WarrantyDraftStatus Status { get; set; } = status;

    /// <summary>Starts a warranty draft in status created.</summary>
    public static WarrantyDraft NewWarrantyDraft(string code, string title)
    {
        return new WarrantyDraft(code, title, new List<string>(), "", 0.0, 0, "", 0, 0, WarrantyDraftStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case WarrantyDraftStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(WarrantyDraftStatus next)
    {
        switch (Status)
        {
            case WarrantyDraftStatus.Created:
                switch (next)
                {
                    case WarrantyDraftStatus.Published:
                    case WarrantyDraftStatus.Retired:
                        return true;
                }
                return false;
            case WarrantyDraftStatus.Validated:
                return next == WarrantyDraftStatus.Published || next == WarrantyDraftStatus.Retired;
            case WarrantyDraftStatus.Published:
                return next == WarrantyDraftStatus.Retired;
        }
        return false;
    }

    public void MoveTo(WarrantyDraftStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"warranty draft {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        StartsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
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

    public double NetExposure()
    {
        return Math.Round((double)Attempts - (double)StartsAt, 2);
    }

    public WarrantyDraft Copy()
    {
        return new WarrantyDraft(
            Code,
            Title,
            Tags,
            Currency,
            FloorValue,
            StartsAt,
            OwnerId,
            Attempts,
            TenureMonths,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
