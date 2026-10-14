namespace Acme.Workforce.Workplace.Visitor.Review;

using Acme.Workforce.Kit.Text;

/// <summary>Where a visitor notice is in its life cycle.</summary>
public enum VisitorNoticeStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>The record of a visitor notice and its current status.</summary>
public sealed class VisitorNotice(string code, string name, double discount, int endsAt, int graceDays, string country, string description, int version, bool visible, int sequence, VisitorNoticeStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public double Discount { get; set; } = discount;
    public int EndsAt { get; set; } = endsAt;
    public int GraceDays { get; set; } = graceDays;
    public string Country { get; set; } = country;
    public string Description { get; set; } = description;
    public int Version { get; set; } = version;
    public bool Visible { get; set; } = visible;
    public int Sequence { get; set; } = sequence;
    public VisitorNoticeStatus Status { get; set; } = status;

    /// <summary>Starts a visitor notice in status requested.</summary>
    public static VisitorNotice NewVisitorNotice(string code, string name)
    {
        return new VisitorNotice(code, name, 0.0, 0, 0, "", "", 0, false, 0, VisitorNoticeStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case VisitorNoticeStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(VisitorNoticeStatus next)
    {
        switch (Status)
        {
            case VisitorNoticeStatus.Requested:
                switch (next)
                {
                    case VisitorNoticeStatus.Granted:
                        return true;
                }
                return false;
            case VisitorNoticeStatus.Granted:
                switch (next)
                {
                    case VisitorNoticeStatus.Revoked:
                        return true;
                }
                return false;
            case VisitorNoticeStatus.Revoked:
                return next == VisitorNoticeStatus.Expired;
        }
        return false;
    }

    public void MoveTo(VisitorNoticeStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"visitor notice {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        EndsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        Version += 1;
    }

    public string Label()
    {
        return $"{Code}: {Name}";
    }

    public double ProjectedValue()
    {
        return Math.Round(Discount * (double)Version, 2);
    }

    public VisitorNotice Copy()
    {
        return new VisitorNotice(
            Code,
            Name,
            Discount,
            EndsAt,
            GraceDays,
            Country,
            Description,
            Version,
            Visible,
            Sequence,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Country);
    }
}
