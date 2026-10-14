namespace Acme.Workforce.Assets.Warranty.Summary;

using Acme.Workforce.Kit.Text;

/// <summary>Where a warranty override is in its life cycle.</summary>
public enum WarrantyOverrideStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>One warranty override, with what the planners need to act on it.</summary>
public sealed class WarrantyOverride(string code, string label, int priority, string location, string title, string locale, int startsAt, string costCenter, string name, WarrantyOverrideStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int Priority { get; set; } = priority;
    public string Location { get; set; } = location;
    public string Title { get; set; } = title;
    public string Locale { get; set; } = locale;
    public int StartsAt { get; set; } = startsAt;
    public string CostCenter { get; set; } = costCenter;
    public string Name { get; set; } = name;
    public WarrantyOverrideStatus Status { get; set; } = status;

    /// <summary>Starts a warranty override in status requested.</summary>
    public static WarrantyOverride NewWarrantyOverride(string code, string label)
    {
        return new WarrantyOverride(code, label, 0, "", "", "", 0, "", "", WarrantyOverrideStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case WarrantyOverrideStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(WarrantyOverrideStatus next)
    {
        switch (Status)
        {
            case WarrantyOverrideStatus.Requested:
                switch (next)
                {
                    case WarrantyOverrideStatus.Granted:
                    case WarrantyOverrideStatus.Revoked:
                        return true;
                }
                return false;
            case WarrantyOverrideStatus.Granted:
                switch (next)
                {
                    case WarrantyOverrideStatus.Revoked:
                    case WarrantyOverrideStatus.Expired:
                        return true;
                }
                return false;
            case WarrantyOverrideStatus.Revoked:
                return next == WarrantyOverrideStatus.Expired;
        }
        return false;
    }

    public void MoveTo(WarrantyOverrideStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"warranty override {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        StartsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{Code} [{Label}]";
    }

    public double NetValue()
    {
        return Math.Round((double)Priority - (double)StartsAt, 2);
    }

    public WarrantyOverride Copy()
    {
        return new WarrantyOverride(
            Code,
            Label,
            Priority,
            Location,
            Title,
            Locale,
            StartsAt,
            CostCenter,
            Name,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Locale);
    }
}
