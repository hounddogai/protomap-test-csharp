namespace Acme.Workforce.Assets.LicenseSeat.Mapping;

using Acme.Workforce.Kit.Text;

/// <summary>Where a license seat mapping is in its life cycle.</summary>
public enum LicenseSeatMappingStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>One license seat mapping, with what the planners need to act on it.</summary>
public sealed class LicenseSeatMapping(string code, string title, double maximum, string category, string region, bool approved, LicenseSeatMappingStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public double Maximum { get; set; } = maximum;
    public string Category { get; set; } = category;
    public string Region { get; set; } = region;
    public bool Approved { get; set; } = approved;
    public LicenseSeatMappingStatus Status { get; set; } = status;

    /// <summary>Starts a license seat mapping in status open.</summary>
    public static LicenseSeatMapping NewLicenseSeatMapping(string code, string title)
    {
        return new LicenseSeatMapping(code, title, 0.0, "", "", false, LicenseSeatMappingStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LicenseSeatMappingStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LicenseSeatMappingStatus next)
    {
        switch (Status)
        {
            case LicenseSeatMappingStatus.Open:
                return next == LicenseSeatMappingStatus.Reopened;
            case LicenseSeatMappingStatus.InReview:
                switch (next)
                {
                    case LicenseSeatMappingStatus.Reopened:
                        return true;
                }
                return false;
            case LicenseSeatMappingStatus.Resolved:
                switch (next)
                {
                    case LicenseSeatMappingStatus.Reopened:
                    case LicenseSeatMappingStatus.Archived:
                        return true;
                }
                return false;
            case LicenseSeatMappingStatus.Reopened:
                switch (next)
                {
                    case LicenseSeatMappingStatus.Archived:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(LicenseSeatMappingStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"license seat mapping {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"license seat mapping"} {Code}";
    }

    public double MeasuredValue()
    {
        return Maximum;
    }

    public LicenseSeatMapping Copy()
    {
        return new LicenseSeatMapping(Code, Title, Maximum, Category, Region, Approved, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Category);
    }
}
