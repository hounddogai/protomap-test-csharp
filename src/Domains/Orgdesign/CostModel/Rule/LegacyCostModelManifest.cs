namespace Acme.Workforce.Orgdesign.CostModel.Rule;

using Acme.Workforce.Kit.Text;

/// <summary>Where a legacy cost model manifest is in its life cycle.</summary>
public enum LegacyCostModelManifestStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>A legacy cost model manifest as the cost model team keeps it.</summary>
public sealed class LegacyCostModelManifest(string code, string title, Dictionary<string, string> attributes, double cost, bool approved, int quantity, string description, string contactEmail, LegacyCostModelManifestStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public double Cost { get; set; } = cost;
    public bool Approved { get; set; } = approved;
    public int Quantity { get; set; } = quantity;
    public string Description { get; set; } = description;
    public string ContactEmail { get; set; } = contactEmail;
    public LegacyCostModelManifestStatus Status { get; set; } = status;

    /// <summary>Starts a legacy cost model manifest in status proposed.</summary>
    public static LegacyCostModelManifest NewLegacyCostModelManifest(string code, string title)
    {
        return new LegacyCostModelManifest(
            code,
            title,
            new Dictionary<string, string>(),
            0.0,
            false,
            0,
            "",
            "",
            LegacyCostModelManifestStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LegacyCostModelManifestStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LegacyCostModelManifestStatus next)
    {
        switch (Status)
        {
            case LegacyCostModelManifestStatus.Proposed:
                switch (next)
                {
                    case LegacyCostModelManifestStatus.Withdrawn:
                        return true;
                }
                return false;
            case LegacyCostModelManifestStatus.Approved:
                switch (next)
                {
                    case LegacyCostModelManifestStatus.Rejected:
                    case LegacyCostModelManifestStatus.Withdrawn:
                        return true;
                }
                return false;
            case LegacyCostModelManifestStatus.Rejected:
                return next == LegacyCostModelManifestStatus.Withdrawn;
        }
        return false;
    }

    public void MoveTo(LegacyCostModelManifestStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"legacy cost model manifest {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} [{Title}]";
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

    public double NetExposure()
    {
        return Math.Round(Cost * (double)Quantity, 2);
    }

    public LegacyCostModelManifest Copy()
    {
        return new LegacyCostModelManifest(
            Code,
            Title,
            Attributes,
            Cost,
            Approved,
            Quantity,
            Description,
            ContactEmail,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
