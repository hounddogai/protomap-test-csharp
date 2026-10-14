namespace Acme.Workforce.Orgdesign.Span.Window;

using Acme.Workforce.Kit.Text;

/// <summary>Where a tiered span group is in its life cycle.</summary>
public enum TieredSpanGroupStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>One tiered span group, with what the planners need to act on it.</summary>
public sealed class TieredSpanGroup(string code, string title, string label, bool visible, string category, double amount, string checksum, List<string> tags, string unit, string source, TieredSpanGroupStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Label { get; set; } = label;
    public bool Visible { get; set; } = visible;
    public string Category { get; set; } = category;
    public double Amount { get; set; } = amount;
    public string Checksum { get; set; } = checksum;
    public List<string> Tags { get; set; } = tags;
    public string Unit { get; set; } = unit;
    public string Source { get; set; } = source;
    public TieredSpanGroupStatus Status { get; set; } = status;

    /// <summary>Starts a tiered span group in status requested.</summary>
    public static TieredSpanGroup NewTieredSpanGroup(string code, string title)
    {
        return new TieredSpanGroup(
            code,
            title,
            "",
            false,
            "",
            0.0,
            "",
            new List<string>(),
            "",
            "",
            TieredSpanGroupStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case TieredSpanGroupStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(TieredSpanGroupStatus next)
    {
        switch (Status)
        {
            case TieredSpanGroupStatus.Requested:
                return next == TieredSpanGroupStatus.Revoked;
            case TieredSpanGroupStatus.Granted:
                switch (next)
                {
                    case TieredSpanGroupStatus.Revoked:
                    case TieredSpanGroupStatus.Expired:
                        return true;
                }
                return false;
            case TieredSpanGroupStatus.Revoked:
                return next == TieredSpanGroupStatus.Expired;
        }
        return false;
    }

    public void MoveTo(TieredSpanGroupStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"tiered span group {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
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

    public double MeasuredValue()
    {
        return Amount;
    }

    public TieredSpanGroup Copy()
    {
        return new TieredSpanGroup(
            Code,
            Title,
            Label,
            Visible,
            Category,
            Amount,
            Checksum,
            Tags,
            Unit,
            Source,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Category);
    }
}
