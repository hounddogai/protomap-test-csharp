namespace Acme.Workforce.Compliance.Audit.Plan;

using Acme.Workforce.Kit.Text;

/// <summary>Where a audit metric is in its life cycle.</summary>
public enum AuditMetricStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>One audit metric, with what the planners need to act on it.</summary>
public sealed class AuditMetric(string code, string label, double minimum, List<string> tags, int updatedAt, int quantity, string source, double cost, string name, int version, int createdAt, AuditMetricStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public double Minimum { get; set; } = minimum;
    public List<string> Tags { get; set; } = tags;
    public int UpdatedAt { get; set; } = updatedAt;
    public int Quantity { get; set; } = quantity;
    public string Source { get; set; } = source;
    public double Cost { get; set; } = cost;
    public string Name { get; set; } = name;
    public int Version { get; set; } = version;
    public int CreatedAt { get; set; } = createdAt;
    public AuditMetricStatus Status { get; set; } = status;

    /// <summary>Starts a audit metric in status created.</summary>
    public static AuditMetric NewAuditMetric(string code, string label)
    {
        return new AuditMetric(
            code,
            label,
            0.0,
            new List<string>(),
            0,
            0,
            "",
            0.0,
            "",
            0,
            0,
            AuditMetricStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AuditMetricStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AuditMetricStatus next)
    {
        switch (Status)
        {
            case AuditMetricStatus.Created:
                return next == AuditMetricStatus.Validated;
            case AuditMetricStatus.Validated:
                switch (next)
                {
                    case AuditMetricStatus.Published:
                    case AuditMetricStatus.Retired:
                        return true;
                }
                return false;
            case AuditMetricStatus.Published:
                switch (next)
                {
                    case AuditMetricStatus.Retired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(AuditMetricStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"audit metric {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        UpdatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        Version += 1;
    }

    public string DisplayLabel()
    {
        return $"{Code} [{Label}]";
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

    public double WeightedExposure()
    {
        return Math.Round((double)UpdatedAt * (double)Quantity, 2);
    }

    public AuditMetric Copy()
    {
        return new AuditMetric(
            Code,
            Label,
            Minimum,
            Tags,
            UpdatedAt,
            Quantity,
            Source,
            Cost,
            Name,
            Version,
            CreatedAt,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Source);
    }
}
