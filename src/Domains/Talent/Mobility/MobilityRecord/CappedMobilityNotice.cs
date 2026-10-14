namespace Acme.Workforce.Talent.Mobility.MobilityRecord;

using Acme.Workforce.Kit.Text;

/// <summary>Where a capped mobility notice is in its life cycle.</summary>
public enum CappedMobilityNoticeStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>A capped mobility notice. Its code is unique within a tenant.</summary>
public sealed class CappedMobilityNotice(string code, string name, Dictionary<string, string> attributes, string category, int attempts, CappedMobilityNoticeStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public string Category { get; set; } = category;
    public int Attempts { get; set; } = attempts;
    public CappedMobilityNoticeStatus Status { get; set; } = status;

    /// <summary>Starts a capped mobility notice in status created.</summary>
    public static CappedMobilityNotice NewCappedMobilityNotice(string code, string name)
    {
        return new CappedMobilityNotice(
            code,
            name,
            new Dictionary<string, string>(),
            "",
            0,
            CappedMobilityNoticeStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case CappedMobilityNoticeStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(CappedMobilityNoticeStatus next)
    {
        switch (Status)
        {
            case CappedMobilityNoticeStatus.Created:
                return next == CappedMobilityNoticeStatus.Retired;
            case CappedMobilityNoticeStatus.Validated:
                return next == CappedMobilityNoticeStatus.Retired;
            case CappedMobilityNoticeStatus.Published:
                return next == CappedMobilityNoticeStatus.Retired;
        }
        return false;
    }

    public void MoveTo(CappedMobilityNoticeStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"capped mobility notice {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"capped mobility notice"} {Code}";
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
        return (double)Attempts;
    }

    public CappedMobilityNotice Copy()
    {
        return new CappedMobilityNotice(Code, Name, Attributes, Category, Attempts, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Category);
    }
}
