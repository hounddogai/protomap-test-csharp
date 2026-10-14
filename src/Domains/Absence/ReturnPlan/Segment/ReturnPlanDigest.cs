namespace Acme.Workforce.Absence.ReturnPlan.Segment;

using Acme.Workforce.Kit.Text;

/// <summary>Where a return plan digest is in its life cycle.</summary>
public enum ReturnPlanDigestStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>The record of a return plan digest and its current status.</summary>
public sealed class ReturnPlanDigest(string code, string name, List<string> tags, int batchSize, int version, string region, string team, bool pinned, double margin, double weight, ReturnPlanDigestStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public List<string> Tags { get; set; } = tags;
    public int BatchSize { get; set; } = batchSize;
    public int Version { get; set; } = version;
    public string Region { get; set; } = region;
    public string Team { get; set; } = team;
    public bool Pinned { get; set; } = pinned;
    public double Margin { get; set; } = margin;
    public double Weight { get; set; } = weight;
    public ReturnPlanDigestStatus Status { get; set; } = status;

    /// <summary>Starts a return plan digest in status idle.</summary>
    public static ReturnPlanDigest NewReturnPlanDigest(string code, string name)
    {
        return new ReturnPlanDigest(
            code,
            name,
            new List<string>(),
            0,
            0,
            "",
            "",
            false,
            0.0,
            0.0,
            ReturnPlanDigestStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ReturnPlanDigestStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ReturnPlanDigestStatus next)
    {
        switch (Status)
        {
            case ReturnPlanDigestStatus.Idle:
                switch (next)
                {
                    case ReturnPlanDigestStatus.Ready:
                        return true;
                }
                return false;
            case ReturnPlanDigestStatus.Warming:
                switch (next)
                {
                    case ReturnPlanDigestStatus.Draining:
                        return true;
                }
                return false;
            case ReturnPlanDigestStatus.Ready:
                return next == ReturnPlanDigestStatus.Draining;
            case ReturnPlanDigestStatus.Draining:
                return next == ReturnPlanDigestStatus.Stopped;
        }
        return false;
    }

    public void MoveTo(ReturnPlanDigestStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"return plan digest {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Version += 1;
    }

    public string Label()
    {
        return $"{Code} ({Name})";
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

    public double EffectiveLoad()
    {
        return Math.Round((double)Version * Margin, 2);
    }

    public ReturnPlanDigest Copy()
    {
        return new ReturnPlanDigest(
            Code,
            Name,
            Tags,
            BatchSize,
            Version,
            Region,
            Team,
            Pinned,
            Margin,
            Weight,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Region);
    }
}
