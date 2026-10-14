namespace Acme.Workforce.Assets.LicenseSeat.RecordItem;

using Acme.Workforce.Kit.Text;

/// <summary>Where a license seat segment is in its life cycle.</summary>
public enum LicenseSeatSegmentStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>A license seat segment. Its code is unique within a tenant.</summary>
public sealed class LicenseSeatSegment(string code, string label, bool pinned, string notes, string currency, bool archived, Dictionary<string, string> attributes, double hours, LicenseSeatSegmentStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public bool Pinned { get; set; } = pinned;
    public string Notes { get; set; } = notes;
    public string Currency { get; set; } = currency;
    public bool Archived { get; set; } = archived;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public double Hours { get; set; } = hours;
    public LicenseSeatSegmentStatus Status { get; set; } = status;

    /// <summary>Starts a license seat segment in status open.</summary>
    public static LicenseSeatSegment NewLicenseSeatSegment(string code, string label)
    {
        return new LicenseSeatSegment(
            code,
            label,
            false,
            "",
            "",
            false,
            new Dictionary<string, string>(),
            0.0,
            LicenseSeatSegmentStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LicenseSeatSegmentStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LicenseSeatSegmentStatus next)
    {
        switch (Status)
        {
            case LicenseSeatSegmentStatus.Open:
                switch (next)
                {
                    case LicenseSeatSegmentStatus.Reopened:
                        return true;
                }
                return false;
            case LicenseSeatSegmentStatus.InReview:
                switch (next)
                {
                    case LicenseSeatSegmentStatus.Resolved:
                    case LicenseSeatSegmentStatus.Archived:
                        return true;
                }
                return false;
            case LicenseSeatSegmentStatus.Resolved:
                return next == LicenseSeatSegmentStatus.Archived;
            case LicenseSeatSegmentStatus.Reopened:
                switch (next)
                {
                    case LicenseSeatSegmentStatus.Archived:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(LicenseSeatSegmentStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"license seat segment {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{"license seat segment"} {Code}";
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
        return Hours;
    }

    public LicenseSeatSegment Copy()
    {
        return new LicenseSeatSegment(Code, Label, Pinned, Notes, Currency, Archived, Attributes, Hours, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
