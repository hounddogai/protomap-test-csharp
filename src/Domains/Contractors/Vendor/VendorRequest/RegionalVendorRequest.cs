namespace Acme.Workforce.Contractors.Vendor.VendorRequest;

using Acme.Workforce.Kit.Text;

/// <summary>Where a regional vendor request is in its life cycle.</summary>
public enum RegionalVendorRequestStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>A regional vendor request as the vendor team keeps it.</summary>
public sealed class RegionalVendorRequest(string code, string title, bool approved, string location, List<string> tags, string skill, string country, string salaryBand, double score, RegionalVendorRequestStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public bool Approved { get; set; } = approved;
    public string Location { get; set; } = location;
    public List<string> Tags { get; set; } = tags;
    public string Skill { get; set; } = skill;
    public string Country { get; set; } = country;
    public string SalaryBand { get; set; } = salaryBand;
    public double Score { get; set; } = score;
    public RegionalVendorRequestStatus Status { get; set; } = status;

    /// <summary>Starts a regional vendor request in status idle.</summary>
    public static RegionalVendorRequest NewRegionalVendorRequest(string code, string title)
    {
        return new RegionalVendorRequest(
            code,
            title,
            false,
            "",
            new List<string>(),
            "",
            "",
            "",
            0.0,
            RegionalVendorRequestStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RegionalVendorRequestStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RegionalVendorRequestStatus next)
    {
        switch (Status)
        {
            case RegionalVendorRequestStatus.Idle:
                switch (next)
                {
                    case RegionalVendorRequestStatus.Draining:
                        return true;
                }
                return false;
            case RegionalVendorRequestStatus.Warming:
                switch (next)
                {
                    case RegionalVendorRequestStatus.Draining:
                        return true;
                }
                return false;
            case RegionalVendorRequestStatus.Ready:
                return next == RegionalVendorRequestStatus.Draining;
            case RegionalVendorRequestStatus.Draining:
                return next == RegionalVendorRequestStatus.Stopped;
        }
        return false;
    }

    public void MoveTo(RegionalVendorRequestStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"regional vendor request {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} / {Title}";
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
        return Score;
    }

    public RegionalVendorRequest Copy()
    {
        return new RegionalVendorRequest(
            Code,
            Title,
            Approved,
            Location,
            Tags,
            Skill,
            Country,
            SalaryBand,
            Score,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Country);
    }
}
