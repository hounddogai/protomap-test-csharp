namespace Acme.Workforce.Orgdesign.CostModel.Manifest;

using Acme.Workforce.Kit.Text;

/// <summary>Where a default cost model manifest is in its life cycle.</summary>
public enum DefaultCostModelManifestStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>A default cost model manifest as the cost model team keeps it.</summary>
public sealed class DefaultCostModelManifest(string code, string title, string externalRef, string costCenter, bool archived, string managerId, int retries, string country, string location, DefaultCostModelManifestStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string ExternalRef { get; set; } = externalRef;
    public string CostCenter { get; set; } = costCenter;
    public bool Archived { get; set; } = archived;
    public string ManagerId { get; set; } = managerId;
    public int Retries { get; set; } = retries;
    public string Country { get; set; } = country;
    public string Location { get; set; } = location;
    public DefaultCostModelManifestStatus Status { get; set; } = status;

    /// <summary>Starts a default cost model manifest in status idle.</summary>
    public static DefaultCostModelManifest NewDefaultCostModelManifest(string code, string title)
    {
        return new DefaultCostModelManifest(
            code,
            title,
            "",
            "",
            false,
            "",
            0,
            "",
            "",
            DefaultCostModelManifestStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case DefaultCostModelManifestStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(DefaultCostModelManifestStatus next)
    {
        switch (Status)
        {
            case DefaultCostModelManifestStatus.Idle:
                switch (next)
                {
                    case DefaultCostModelManifestStatus.Stopped:
                        return true;
                }
                return false;
            case DefaultCostModelManifestStatus.Warming:
                switch (next)
                {
                    case DefaultCostModelManifestStatus.Draining:
                    case DefaultCostModelManifestStatus.Stopped:
                        return true;
                }
                return false;
            case DefaultCostModelManifestStatus.Ready:
                return next == DefaultCostModelManifestStatus.Stopped;
            case DefaultCostModelManifestStatus.Draining:
                switch (next)
                {
                    case DefaultCostModelManifestStatus.Stopped:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(DefaultCostModelManifestStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"default cost model manifest {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} / {Title}";
    }

    public double MeasuredValue()
    {
        return (double)Retries;
    }

    public DefaultCostModelManifest Copy()
    {
        return new DefaultCostModelManifest(
            Code,
            Title,
            ExternalRef,
            CostCenter,
            Archived,
            ManagerId,
            Retries,
            Country,
            Location,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Country);
    }
}
