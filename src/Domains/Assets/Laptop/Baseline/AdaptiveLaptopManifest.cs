namespace Acme.Workforce.Assets.Laptop.Baseline;

using Acme.Workforce.Kit.Text;

/// <summary>Where a adaptive laptop manifest is in its life cycle.</summary>
public enum AdaptiveLaptopManifestStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>One adaptive laptop manifest, with what the planners need to act on it.</summary>
public sealed class AdaptiveLaptopManifest(string code, string title, string grade, string externalRef, double floorValue, string location, bool active, string source, string timezone, string employeeId, string label, AdaptiveLaptopManifestStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Grade { get; set; } = grade;
    public string ExternalRef { get; set; } = externalRef;
    public double FloorValue { get; set; } = floorValue;
    public string Location { get; set; } = location;
    public bool Active { get; set; } = active;
    public string Source { get; set; } = source;
    public string Timezone { get; set; } = timezone;
    public string EmployeeId { get; set; } = employeeId;
    public string Label { get; set; } = label;
    public AdaptiveLaptopManifestStatus Status { get; set; } = status;

    /// <summary>Starts a adaptive laptop manifest in status requested.</summary>
    public static AdaptiveLaptopManifest NewAdaptiveLaptopManifest(string code, string title)
    {
        return new AdaptiveLaptopManifest(
            code,
            title,
            "",
            "",
            0.0,
            "",
            false,
            "",
            "",
            "",
            "",
            AdaptiveLaptopManifestStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AdaptiveLaptopManifestStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AdaptiveLaptopManifestStatus next)
    {
        switch (Status)
        {
            case AdaptiveLaptopManifestStatus.Requested:
                switch (next)
                {
                    case AdaptiveLaptopManifestStatus.Expired:
                        return true;
                }
                return false;
            case AdaptiveLaptopManifestStatus.Granted:
                switch (next)
                {
                    case AdaptiveLaptopManifestStatus.Revoked:
                    case AdaptiveLaptopManifestStatus.Expired:
                        return true;
                }
                return false;
            case AdaptiveLaptopManifestStatus.Revoked:
                return next == AdaptiveLaptopManifestStatus.Expired;
        }
        return false;
    }

    public void MoveTo(AdaptiveLaptopManifestStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"adaptive laptop manifest {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{"adaptive laptop manifest"} {Code}";
    }

    public double MeasuredValue()
    {
        return FloorValue;
    }

    public AdaptiveLaptopManifest Copy()
    {
        return new AdaptiveLaptopManifest(
            Code,
            Title,
            Grade,
            ExternalRef,
            FloorValue,
            Location,
            Active,
            Source,
            Timezone,
            EmployeeId,
            Label,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Source);
    }
}
