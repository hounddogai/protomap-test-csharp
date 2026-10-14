namespace Acme.Workforce.Payrollops.Garnishment.Baseline;

using Acme.Workforce.Kit.Text;

/// <summary>Where a fallback garnishment record is in its life cycle.</summary>
public enum FallbackGarnishmentRecordStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>One fallback garnishment record, with what the planners need to act on it.</summary>
public sealed class FallbackGarnishmentRecord(string code, string title, string ownerId, string country, string managerId, string currency, Dictionary<string, string> attributes, double fte, string channel, double weight, double margin, FallbackGarnishmentRecordStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string OwnerId { get; set; } = ownerId;
    public string Country { get; set; } = country;
    public string ManagerId { get; set; } = managerId;
    public string Currency { get; set; } = currency;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public double Fte { get; set; } = fte;
    public string Channel { get; set; } = channel;
    public double Weight { get; set; } = weight;
    public double Margin { get; set; } = margin;
    public FallbackGarnishmentRecordStatus Status { get; set; } = status;

    /// <summary>Starts a fallback garnishment record in status draft.</summary>
    public static FallbackGarnishmentRecord NewFallbackGarnishmentRecord(string code, string title)
    {
        return new FallbackGarnishmentRecord(
            code,
            title,
            "",
            "",
            "",
            "",
            new Dictionary<string, string>(),
            0.0,
            "",
            0.0,
            0.0,
            FallbackGarnishmentRecordStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case FallbackGarnishmentRecordStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(FallbackGarnishmentRecordStatus next)
    {
        switch (Status)
        {
            case FallbackGarnishmentRecordStatus.Draft:
                return next == FallbackGarnishmentRecordStatus.Suspended;
            case FallbackGarnishmentRecordStatus.Pending:
                switch (next)
                {
                    case FallbackGarnishmentRecordStatus.Active:
                        return true;
                }
                return false;
            case FallbackGarnishmentRecordStatus.Active:
                switch (next)
                {
                    case FallbackGarnishmentRecordStatus.Suspended:
                        return true;
                }
                return false;
            case FallbackGarnishmentRecordStatus.Suspended:
                return next == FallbackGarnishmentRecordStatus.Closed;
        }
        return false;
    }

    public void MoveTo(FallbackGarnishmentRecordStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"fallback garnishment record {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"fallback garnishment record"} {Code}";
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

    public double WeightedValue()
    {
        return Math.Round(Weight + Fte, 2);
    }

    public FallbackGarnishmentRecord Copy()
    {
        return new FallbackGarnishmentRecord(
            Code,
            Title,
            OwnerId,
            Country,
            ManagerId,
            Currency,
            Attributes,
            Fte,
            Channel,
            Weight,
            Margin,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Country);
    }
}
