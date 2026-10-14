namespace Acme.Workforce.Absence.ReturnPlan.RollingPlanManifest;

using Acme.Workforce.Kit.Text;

/// <summary>Where a batched return plan manifest is in its life cycle.</summary>
public enum BatchedReturnPlanManifestStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>One batched return plan manifest, with what the planners need to act on it.</summary>
public sealed class BatchedReturnPlanManifest(string code, string name, string checksum, string description, double threshold, double rating, double maximum, string segment, int updatedAt, string channel, string location, BatchedReturnPlanManifestStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Checksum { get; set; } = checksum;
    public string Description { get; set; } = description;
    public double Threshold { get; set; } = threshold;
    public double Rating { get; set; } = rating;
    public double Maximum { get; set; } = maximum;
    public string Segment { get; set; } = segment;
    public int UpdatedAt { get; set; } = updatedAt;
    public string Channel { get; set; } = channel;
    public string Location { get; set; } = location;
    public BatchedReturnPlanManifestStatus Status { get; set; } = status;

    /// <summary>Starts a batched return plan manifest in status new.</summary>
    public static BatchedReturnPlanManifest NewBatchedReturnPlanManifest(string code, string name)
    {
        return new BatchedReturnPlanManifest(
            code,
            name,
            "",
            "",
            0.0,
            0.0,
            0.0,
            "",
            0,
            "",
            "",
            BatchedReturnPlanManifestStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case BatchedReturnPlanManifestStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(BatchedReturnPlanManifestStatus next)
    {
        switch (Status)
        {
            case BatchedReturnPlanManifestStatus.New:
                return next == BatchedReturnPlanManifestStatus.Queued || next == BatchedReturnPlanManifestStatus.Done;
            case BatchedReturnPlanManifestStatus.Queued:
                return next == BatchedReturnPlanManifestStatus.Running || next == BatchedReturnPlanManifestStatus.Failed;
            case BatchedReturnPlanManifestStatus.Running:
                switch (next)
                {
                    case BatchedReturnPlanManifestStatus.Done:
                    case BatchedReturnPlanManifestStatus.Failed:
                        return true;
                }
                return false;
            case BatchedReturnPlanManifestStatus.Done:
                switch (next)
                {
                    case BatchedReturnPlanManifestStatus.Failed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(BatchedReturnPlanManifestStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"batched return plan manifest {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        UpdatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{"batched return plan manifest"} {Code}";
    }

    public double NetTotal()
    {
        return Math.Round(Rating * Maximum, 2);
    }

    public BatchedReturnPlanManifest Copy()
    {
        return new BatchedReturnPlanManifest(
            Code,
            Name,
            Checksum,
            Description,
            Threshold,
            Rating,
            Maximum,
            Segment,
            UpdatedAt,
            Channel,
            Location,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Segment);
    }
}
