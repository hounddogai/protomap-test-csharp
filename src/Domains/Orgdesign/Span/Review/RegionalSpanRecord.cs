namespace Acme.Workforce.Orgdesign.Span.Review;

using Acme.Workforce.Kit.Text;

/// <summary>Where a regional span record is in its life cycle.</summary>
public enum RegionalSpanRecordStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>One regional span record, with what the planners need to act on it.</summary>
public sealed class RegionalSpanRecord(string code, string title, int revision, Dictionary<string, string> attributes, double ratio, string reviewer, int batchSize, int version, string channel, int updatedAt, RegionalSpanRecordStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int Revision { get; set; } = revision;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public double Ratio { get; set; } = ratio;
    public string Reviewer { get; set; } = reviewer;
    public int BatchSize { get; set; } = batchSize;
    public int Version { get; set; } = version;
    public string Channel { get; set; } = channel;
    public int UpdatedAt { get; set; } = updatedAt;
    public RegionalSpanRecordStatus Status { get; set; } = status;

    /// <summary>Starts a regional span record in status created.</summary>
    public static RegionalSpanRecord NewRegionalSpanRecord(string code, string title)
    {
        return new RegionalSpanRecord(
            code,
            title,
            0,
            new Dictionary<string, string>(),
            0.0,
            "",
            0,
            0,
            "",
            0,
            RegionalSpanRecordStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RegionalSpanRecordStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RegionalSpanRecordStatus next)
    {
        switch (Status)
        {
            case RegionalSpanRecordStatus.Created:
                return next == RegionalSpanRecordStatus.Validated || next == RegionalSpanRecordStatus.Retired;
            case RegionalSpanRecordStatus.Validated:
                return next == RegionalSpanRecordStatus.Published || next == RegionalSpanRecordStatus.Retired;
            case RegionalSpanRecordStatus.Published:
                switch (next)
                {
                    case RegionalSpanRecordStatus.Retired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(RegionalSpanRecordStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"regional span record {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        UpdatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        Revision += 1;
    }

    public string Label()
    {
        return $"{Code} ({Title})";
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

    public double EffectiveValue()
    {
        return Math.Round((double)Version + Ratio, 2);
    }

    public RegionalSpanRecord Copy()
    {
        return new RegionalSpanRecord(
            Code,
            Title,
            Revision,
            Attributes,
            Ratio,
            Reviewer,
            BatchSize,
            Version,
            Channel,
            UpdatedAt,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Channel);
    }
}
