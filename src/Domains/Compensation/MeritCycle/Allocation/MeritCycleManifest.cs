namespace Acme.Workforce.Compensation.MeritCycle.Allocation;

using Acme.Workforce.Kit.Text;

/// <summary>Where a merit cycle manifest is in its life cycle.</summary>
public enum MeritCycleManifestStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>The record of a merit cycle manifest and its current status.</summary>
public sealed class MeritCycleManifest(string code, string title, Dictionary<string, string> attributes, double discount, string notes, string costCenter, string name, string employeeId, int updatedAt, int revision, int priority, MeritCycleManifestStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public double Discount { get; set; } = discount;
    public string Notes { get; set; } = notes;
    public string CostCenter { get; set; } = costCenter;
    public string Name { get; set; } = name;
    public string EmployeeId { get; set; } = employeeId;
    public int UpdatedAt { get; set; } = updatedAt;
    public int Revision { get; set; } = revision;
    public int Priority { get; set; } = priority;
    public MeritCycleManifestStatus Status { get; set; } = status;

    /// <summary>Starts a merit cycle manifest in status open.</summary>
    public static MeritCycleManifest NewMeritCycleManifest(string code, string title)
    {
        return new MeritCycleManifest(
            code,
            title,
            new Dictionary<string, string>(),
            0.0,
            "",
            "",
            "",
            "",
            0,
            0,
            0,
            MeritCycleManifestStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case MeritCycleManifestStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(MeritCycleManifestStatus next)
    {
        switch (Status)
        {
            case MeritCycleManifestStatus.Open:
                switch (next)
                {
                    case MeritCycleManifestStatus.InReview:
                        return true;
                }
                return false;
            case MeritCycleManifestStatus.InReview:
                switch (next)
                {
                    case MeritCycleManifestStatus.Archived:
                        return true;
                }
                return false;
            case MeritCycleManifestStatus.Resolved:
                switch (next)
                {
                    case MeritCycleManifestStatus.Reopened:
                    case MeritCycleManifestStatus.Archived:
                        return true;
                }
                return false;
            case MeritCycleManifestStatus.Reopened:
                return next == MeritCycleManifestStatus.Archived;
        }
        return false;
    }

    public void MoveTo(MeritCycleManifestStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"merit cycle manifest {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        UpdatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        Revision += 1;
    }

    public string Label()
    {
        return $"{"merit cycle manifest"} {Code}";
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

    public double WeightedVolume()
    {
        return Math.Round((double)UpdatedAt + Discount, 2);
    }

    public MeritCycleManifest Copy()
    {
        return new MeritCycleManifest(
            Code,
            Title,
            Attributes,
            Discount,
            Notes,
            CostCenter,
            Name,
            EmployeeId,
            UpdatedAt,
            Revision,
            Priority,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
