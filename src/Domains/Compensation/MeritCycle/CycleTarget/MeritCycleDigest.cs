namespace Acme.Workforce.Compensation.MeritCycle.CycleTarget;

using Acme.Workforce.Kit.Text;

/// <summary>Where a merit cycle digest is in its life cycle.</summary>
public enum MeritCycleDigestStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>A merit cycle digest. Its code is unique within a tenant.</summary>
public sealed class MeritCycleDigest(string code, string name, string description, List<string> tags, bool pinned, double ratio, int updatedAt, string category, int version, string salaryBand, double weight, MeritCycleDigestStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Description { get; set; } = description;
    public List<string> Tags { get; set; } = tags;
    public bool Pinned { get; set; } = pinned;
    public double Ratio { get; set; } = ratio;
    public int UpdatedAt { get; set; } = updatedAt;
    public string Category { get; set; } = category;
    public int Version { get; set; } = version;
    public string SalaryBand { get; set; } = salaryBand;
    public double Weight { get; set; } = weight;
    public MeritCycleDigestStatus Status { get; set; } = status;

    /// <summary>Starts a merit cycle digest in status idle.</summary>
    public static MeritCycleDigest NewMeritCycleDigest(string code, string name)
    {
        return new MeritCycleDigest(
            code,
            name,
            "",
            new List<string>(),
            false,
            0.0,
            0,
            "",
            0,
            "",
            0.0,
            MeritCycleDigestStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case MeritCycleDigestStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(MeritCycleDigestStatus next)
    {
        switch (Status)
        {
            case MeritCycleDigestStatus.Idle:
                return next == MeritCycleDigestStatus.Stopped;
            case MeritCycleDigestStatus.Warming:
                return next == MeritCycleDigestStatus.Draining || next == MeritCycleDigestStatus.Stopped;
            case MeritCycleDigestStatus.Ready:
                switch (next)
                {
                    case MeritCycleDigestStatus.Stopped:
                        return true;
                }
                return false;
            case MeritCycleDigestStatus.Draining:
                return next == MeritCycleDigestStatus.Stopped;
        }
        return false;
    }

    public void MoveTo(MeritCycleDigestStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"merit cycle digest {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        UpdatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        Version += 1;
    }

    public string Label()
    {
        return $"{"merit cycle digest"} {Code}";
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

    public double AdjustedExposure()
    {
        return Math.Round((double)UpdatedAt + (double)Version, 2);
    }

    public MeritCycleDigest Copy()
    {
        return new MeritCycleDigest(
            Code,
            Name,
            Description,
            Tags,
            Pinned,
            Ratio,
            UpdatedAt,
            Category,
            Version,
            SalaryBand,
            Weight,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Category);
    }
}
