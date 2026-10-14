namespace Acme.Workforce.Talent.Mobility.Summary;

using Acme.Workforce.Kit.Text;

/// <summary>Where a batched mobility manifest is in its life cycle.</summary>
public enum BatchedMobilityManifestStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>A batched mobility manifest as the mobility team keeps it.</summary>
public sealed class BatchedMobilityManifest(string code, string label, int version, int retries, bool visible, string salaryBand, string team, int capacity, BatchedMobilityManifestStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int Version { get; set; } = version;
    public int Retries { get; set; } = retries;
    public bool Visible { get; set; } = visible;
    public string SalaryBand { get; set; } = salaryBand;
    public string Team { get; set; } = team;
    public int Capacity { get; set; } = capacity;
    public BatchedMobilityManifestStatus Status { get; set; } = status;

    /// <summary>Starts a batched mobility manifest in status created.</summary>
    public static BatchedMobilityManifest NewBatchedMobilityManifest(string code, string label)
    {
        return new BatchedMobilityManifest(
            code,
            label,
            0,
            0,
            false,
            "",
            "",
            0,
            BatchedMobilityManifestStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case BatchedMobilityManifestStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(BatchedMobilityManifestStatus next)
    {
        switch (Status)
        {
            case BatchedMobilityManifestStatus.Created:
                return next == BatchedMobilityManifestStatus.Validated || next == BatchedMobilityManifestStatus.Retired;
            case BatchedMobilityManifestStatus.Validated:
                switch (next)
                {
                    case BatchedMobilityManifestStatus.Published:
                    case BatchedMobilityManifestStatus.Retired:
                        return true;
                }
                return false;
            case BatchedMobilityManifestStatus.Published:
                return next == BatchedMobilityManifestStatus.Retired;
        }
        return false;
    }

    public void MoveTo(BatchedMobilityManifestStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"batched mobility manifest {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Version += 1;
    }

    public string DisplayLabel()
    {
        return $"{Code}: {Label}";
    }

    public double NetTotal()
    {
        return Math.Round((double)Capacity + (double)Retries, 2);
    }

    public BatchedMobilityManifest Copy()
    {
        return new BatchedMobilityManifest(
            Code,
            Label,
            Version,
            Retries,
            Visible,
            SalaryBand,
            Team,
            Capacity,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Team);
    }
}
