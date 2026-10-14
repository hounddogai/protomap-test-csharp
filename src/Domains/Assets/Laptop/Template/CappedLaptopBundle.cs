namespace Acme.Workforce.Assets.Laptop.Template;

using Acme.Workforce.Kit.Text;

/// <summary>Where a capped laptop bundle is in its life cycle.</summary>
public enum CappedLaptopBundleStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>One capped laptop bundle, with what the planners need to act on it.</summary>
public sealed class CappedLaptopBundle(string code, string title, bool approved, string checksum, string category, string team, List<string> tags, double discount, CappedLaptopBundleStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public bool Approved { get; set; } = approved;
    public string Checksum { get; set; } = checksum;
    public string Category { get; set; } = category;
    public string Team { get; set; } = team;
    public List<string> Tags { get; set; } = tags;
    public double Discount { get; set; } = discount;
    public CappedLaptopBundleStatus Status { get; set; } = status;

    /// <summary>Starts a capped laptop bundle in status requested.</summary>
    public static CappedLaptopBundle NewCappedLaptopBundle(string code, string title)
    {
        return new CappedLaptopBundle(
            code,
            title,
            false,
            "",
            "",
            "",
            new List<string>(),
            0.0,
            CappedLaptopBundleStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case CappedLaptopBundleStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(CappedLaptopBundleStatus next)
    {
        switch (Status)
        {
            case CappedLaptopBundleStatus.Requested:
                return next == CappedLaptopBundleStatus.Revoked || next == CappedLaptopBundleStatus.Expired;
            case CappedLaptopBundleStatus.Granted:
                switch (next)
                {
                    case CappedLaptopBundleStatus.Revoked:
                    case CappedLaptopBundleStatus.Expired:
                        return true;
                }
                return false;
            case CappedLaptopBundleStatus.Revoked:
                return next == CappedLaptopBundleStatus.Expired;
        }
        return false;
    }

    public void MoveTo(CappedLaptopBundleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"capped laptop bundle {Code} cannot move from {Status} to {next}");
        }
        Status = next;
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

    public double MeasuredValue()
    {
        return Discount;
    }

    public CappedLaptopBundle Copy()
    {
        return new CappedLaptopBundle(Code, Title, Approved, Checksum, Category, Team, Tags, Discount, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Category);
    }
}
