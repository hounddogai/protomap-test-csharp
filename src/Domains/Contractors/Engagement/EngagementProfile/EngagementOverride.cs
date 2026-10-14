namespace Acme.Workforce.Contractors.Engagement.EngagementProfile;

using Acme.Workforce.Kit.Text;

/// <summary>Where a engagement override is in its life cycle.</summary>
public enum EngagementOverrideStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>A engagement override. Its code is unique within a tenant.</summary>
public sealed class EngagementOverride(string code, string name, int leaveDays, string location, string source, string salaryBand, bool pinned, int capacity, string externalRef, EngagementOverrideStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int LeaveDays { get; set; } = leaveDays;
    public string Location { get; set; } = location;
    public string Source { get; set; } = source;
    public string SalaryBand { get; set; } = salaryBand;
    public bool Pinned { get; set; } = pinned;
    public int Capacity { get; set; } = capacity;
    public string ExternalRef { get; set; } = externalRef;
    public EngagementOverrideStatus Status { get; set; } = status;

    /// <summary>Starts a engagement override in status created.</summary>
    public static EngagementOverride NewEngagementOverride(string code, string name)
    {
        return new EngagementOverride(code, name, 0, "", "", "", false, 0, "", EngagementOverrideStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case EngagementOverrideStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(EngagementOverrideStatus next)
    {
        switch (Status)
        {
            case EngagementOverrideStatus.Created:
                return next == EngagementOverrideStatus.Validated || next == EngagementOverrideStatus.Published;
            case EngagementOverrideStatus.Validated:
                return next == EngagementOverrideStatus.Retired;
            case EngagementOverrideStatus.Published:
                switch (next)
                {
                    case EngagementOverrideStatus.Retired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(EngagementOverrideStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"engagement override {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code}: {Name}";
    }

    public double NetExposure()
    {
        return Math.Round((double)Capacity - (double)LeaveDays, 2);
    }

    public EngagementOverride Copy()
    {
        return new EngagementOverride(
            Code,
            Name,
            LeaveDays,
            Location,
            Source,
            SalaryBand,
            Pinned,
            Capacity,
            ExternalRef,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Source);
    }
}
