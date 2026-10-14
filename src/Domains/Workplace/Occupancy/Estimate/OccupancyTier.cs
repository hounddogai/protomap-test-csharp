namespace Acme.Workforce.Workplace.Occupancy.Estimate;

using Acme.Workforce.Kit.Text;

/// <summary>Where a occupancy tier is in its life cycle.</summary>
public enum OccupancyTierStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>The record of a occupancy tier and its current status.</summary>
public sealed class OccupancyTier(string code, string title, string badge, string locale, string costCenter, Dictionary<string, string> attributes, int revision, int windowDays, string unit, OccupancyTierStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Badge { get; set; } = badge;
    public string Locale { get; set; } = locale;
    public string CostCenter { get; set; } = costCenter;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public int Revision { get; set; } = revision;
    public int WindowDays { get; set; } = windowDays;
    public string Unit { get; set; } = unit;
    public OccupancyTierStatus Status { get; set; } = status;

    /// <summary>Starts a occupancy tier in status created.</summary>
    public static OccupancyTier NewOccupancyTier(string code, string title)
    {
        return new OccupancyTier(
            code,
            title,
            "",
            "",
            "",
            new Dictionary<string, string>(),
            0,
            0,
            "",
            OccupancyTierStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case OccupancyTierStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(OccupancyTierStatus next)
    {
        switch (Status)
        {
            case OccupancyTierStatus.Created:
                switch (next)
                {
                    case OccupancyTierStatus.Validated:
                    case OccupancyTierStatus.Published:
                        return true;
                }
                return false;
            case OccupancyTierStatus.Validated:
                switch (next)
                {
                    case OccupancyTierStatus.Published:
                        return true;
                }
                return false;
            case OccupancyTierStatus.Published:
                return next == OccupancyTierStatus.Retired;
        }
        return false;
    }

    public void MoveTo(OccupancyTierStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"occupancy tier {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Revision += 1;
    }

    public string Label()
    {
        return $"{Code} / {Title}";
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

    public double ProjectedExposure()
    {
        return Math.Round((double)Revision * (double)WindowDays, 2);
    }

    public OccupancyTier Copy()
    {
        return new OccupancyTier(
            Code,
            Title,
            Badge,
            Locale,
            CostCenter,
            Attributes,
            Revision,
            WindowDays,
            Unit,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Locale);
    }
}
