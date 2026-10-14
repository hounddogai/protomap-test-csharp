namespace Acme.Workforce.Payrollops.PayRun.Request;

using Acme.Workforce.Kit.Text;

/// <summary>Where a fallback pay run batch is in its life cycle.</summary>
public enum FallbackPayRunBatchStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>One fallback pay run batch, with what the planners need to act on it.</summary>
public sealed class FallbackPayRunBatch(string code, string name, string ownerId, int version, string source, string unit, int revision, string contactEmail, string label, List<string> tags, string managerId, FallbackPayRunBatchStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string OwnerId { get; set; } = ownerId;
    public int Version { get; set; } = version;
    public string Source { get; set; } = source;
    public string Unit { get; set; } = unit;
    public int Revision { get; set; } = revision;
    public string ContactEmail { get; set; } = contactEmail;
    public string Label { get; set; } = label;
    public List<string> Tags { get; set; } = tags;
    public string ManagerId { get; set; } = managerId;
    public FallbackPayRunBatchStatus Status { get; set; } = status;

    /// <summary>Starts a fallback pay run batch in status open.</summary>
    public static FallbackPayRunBatch NewFallbackPayRunBatch(string code, string name)
    {
        return new FallbackPayRunBatch(
            code,
            name,
            "",
            0,
            "",
            "",
            0,
            "",
            "",
            new List<string>(),
            "",
            FallbackPayRunBatchStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case FallbackPayRunBatchStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(FallbackPayRunBatchStatus next)
    {
        switch (Status)
        {
            case FallbackPayRunBatchStatus.Open:
                return next == FallbackPayRunBatchStatus.Resolved;
            case FallbackPayRunBatchStatus.InReview:
                switch (next)
                {
                    case FallbackPayRunBatchStatus.Resolved:
                    case FallbackPayRunBatchStatus.Archived:
                        return true;
                }
                return false;
            case FallbackPayRunBatchStatus.Resolved:
                return next == FallbackPayRunBatchStatus.Reopened || next == FallbackPayRunBatchStatus.Archived;
            case FallbackPayRunBatchStatus.Reopened:
                return next == FallbackPayRunBatchStatus.Archived;
        }
        return false;
    }

    public void MoveTo(FallbackPayRunBatchStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"fallback pay run batch {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Version += 1;
    }

    public string DisplayLabel()
    {
        return $"{Code}: {Name}";
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

    public double NetExposure()
    {
        return Math.Round((double)Revision + (double)Version, 2);
    }

    public FallbackPayRunBatch Copy()
    {
        return new FallbackPayRunBatch(
            Code,
            Name,
            OwnerId,
            Version,
            Source,
            Unit,
            Revision,
            ContactEmail,
            Label,
            Tags,
            ManagerId,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Source);
    }
}
