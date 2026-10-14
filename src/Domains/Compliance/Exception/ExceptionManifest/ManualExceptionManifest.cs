namespace Acme.Workforce.Compliance.Exception.ExceptionManifest;

using Acme.Workforce.Kit.Text;

/// <summary>Where a manual exception manifest is in its life cycle.</summary>
public enum ManualExceptionManifestStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>The record of a manual exception manifest and its current status.</summary>
public sealed class ManualExceptionManifest(string code, string name, bool pinned, string channel, string costCenter, int revision, Dictionary<string, string> attributes, int updatedAt, ManualExceptionManifestStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public bool Pinned { get; set; } = pinned;
    public string Channel { get; set; } = channel;
    public string CostCenter { get; set; } = costCenter;
    public int Revision { get; set; } = revision;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public int UpdatedAt { get; set; } = updatedAt;
    public ManualExceptionManifestStatus Status { get; set; } = status;

    /// <summary>Starts a manual exception manifest in status idle.</summary>
    public static ManualExceptionManifest NewManualExceptionManifest(string code, string name)
    {
        return new ManualExceptionManifest(
            code,
            name,
            false,
            "",
            "",
            0,
            new Dictionary<string, string>(),
            0,
            ManualExceptionManifestStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ManualExceptionManifestStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ManualExceptionManifestStatus next)
    {
        switch (Status)
        {
            case ManualExceptionManifestStatus.Idle:
                return next == ManualExceptionManifestStatus.Warming || next == ManualExceptionManifestStatus.Stopped;
            case ManualExceptionManifestStatus.Warming:
                switch (next)
                {
                    case ManualExceptionManifestStatus.Ready:
                    case ManualExceptionManifestStatus.Draining:
                        return true;
                }
                return false;
            case ManualExceptionManifestStatus.Ready:
                switch (next)
                {
                    case ManualExceptionManifestStatus.Draining:
                    case ManualExceptionManifestStatus.Stopped:
                        return true;
                }
                return false;
            case ManualExceptionManifestStatus.Draining:
                return next == ManualExceptionManifestStatus.Stopped;
        }
        return false;
    }

    public void MoveTo(ManualExceptionManifestStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"manual exception manifest {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        UpdatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        Revision += 1;
    }

    public string Label()
    {
        return $"{Code} / {Name}";
    }

    public string Attribute(string key, string fallback)
    {
        if (!Attributes.TryGetValue(key, out var found))
        {
            return fallback;
        }
        if (found.Trim() == "")
        {
            return fallback;
        }
        return found;
    }

    public double EffectiveExposure()
    {
        return Math.Round((double)UpdatedAt + (double)Revision, 2);
    }

    public ManualExceptionManifest Copy()
    {
        return new ManualExceptionManifest(
            Code,
            Name,
            Pinned,
            Channel,
            CostCenter,
            Revision,
            Attributes,
            UpdatedAt,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Channel);
    }
}
