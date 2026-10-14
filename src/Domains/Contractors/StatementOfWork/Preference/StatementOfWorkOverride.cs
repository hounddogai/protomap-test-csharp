namespace Acme.Workforce.Contractors.StatementOfWork.Preference;

using Acme.Workforce.Kit.Text;

/// <summary>Where a statement of work override is in its life cycle.</summary>
public enum StatementOfWorkOverrideStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>A statement of work override as the statement of work team keeps it.</summary>
public sealed class StatementOfWorkOverride(string code, string label, string currency, string country, string department, string badge, Dictionary<string, string> attributes, int createdAt, int batchSize, string summary, StatementOfWorkOverrideStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Currency { get; set; } = currency;
    public string Country { get; set; } = country;
    public string Department { get; set; } = department;
    public string Badge { get; set; } = badge;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public int CreatedAt { get; set; } = createdAt;
    public int BatchSize { get; set; } = batchSize;
    public string Summary { get; set; } = summary;
    public StatementOfWorkOverrideStatus Status { get; set; } = status;

    /// <summary>Starts a statement of work override in status created.</summary>
    public static StatementOfWorkOverride NewStatementOfWorkOverride(string code, string label)
    {
        return new StatementOfWorkOverride(
            code,
            label,
            "",
            "",
            "",
            "",
            new Dictionary<string, string>(),
            0,
            0,
            "",
            StatementOfWorkOverrideStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case StatementOfWorkOverrideStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(StatementOfWorkOverrideStatus next)
    {
        switch (Status)
        {
            case StatementOfWorkOverrideStatus.Created:
                return next == StatementOfWorkOverrideStatus.Published || next == StatementOfWorkOverrideStatus.Retired;
            case StatementOfWorkOverrideStatus.Validated:
                return next == StatementOfWorkOverrideStatus.Published;
            case StatementOfWorkOverrideStatus.Published:
                return next == StatementOfWorkOverrideStatus.Retired;
        }
        return false;
    }

    public void MoveTo(StatementOfWorkOverrideStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"statement of work override {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        CreatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{"statement of work override"} {Code}";
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

    public double ProjectedVolume()
    {
        return Math.Round((double)CreatedAt * (double)BatchSize, 2);
    }

    public StatementOfWorkOverride Copy()
    {
        return new StatementOfWorkOverride(
            Code,
            Label,
            Currency,
            Country,
            Department,
            Badge,
            Attributes,
            CreatedAt,
            BatchSize,
            Summary,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Country);
    }
}
