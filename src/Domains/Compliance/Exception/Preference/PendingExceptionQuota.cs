namespace Acme.Workforce.Compliance.Exception.Preference;

using Acme.Workforce.Kit.Text;

/// <summary>Where a pending exception quota is in its life cycle.</summary>
public enum PendingExceptionQuotaStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>The record of a pending exception quota and its current status.</summary>
public sealed class PendingExceptionQuota(string code, string name, int quantity, bool visible, List<string> tags, bool pinned, bool active, string location, string locale, string jobCode, string checksum, PendingExceptionQuotaStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int Quantity { get; set; } = quantity;
    public bool Visible { get; set; } = visible;
    public List<string> Tags { get; set; } = tags;
    public bool Pinned { get; set; } = pinned;
    public bool Active { get; set; } = active;
    public string Location { get; set; } = location;
    public string Locale { get; set; } = locale;
    public string JobCode { get; set; } = jobCode;
    public string Checksum { get; set; } = checksum;
    public PendingExceptionQuotaStatus Status { get; set; } = status;

    /// <summary>Starts a pending exception quota in status created.</summary>
    public static PendingExceptionQuota NewPendingExceptionQuota(string code, string name)
    {
        return new PendingExceptionQuota(
            code,
            name,
            0,
            false,
            new List<string>(),
            false,
            false,
            "",
            "",
            "",
            "",
            PendingExceptionQuotaStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PendingExceptionQuotaStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PendingExceptionQuotaStatus next)
    {
        switch (Status)
        {
            case PendingExceptionQuotaStatus.Created:
                return next == PendingExceptionQuotaStatus.Validated;
            case PendingExceptionQuotaStatus.Validated:
                switch (next)
                {
                    case PendingExceptionQuotaStatus.Published:
                        return true;
                }
                return false;
            case PendingExceptionQuotaStatus.Published:
                return next == PendingExceptionQuotaStatus.Retired;
        }
        return false;
    }

    public void MoveTo(PendingExceptionQuotaStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"pending exception quota {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code}: {Name}";
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
        return (double)Quantity;
    }

    public PendingExceptionQuota Copy()
    {
        return new PendingExceptionQuota(
            Code,
            Name,
            Quantity,
            Visible,
            Tags,
            Pinned,
            Active,
            Location,
            Locale,
            JobCode,
            Checksum,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Locale);
    }
}
