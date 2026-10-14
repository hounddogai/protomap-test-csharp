namespace Acme.Workforce.Workplace.Occupancy.Reservation;

using Acme.Workforce.Kit.Text;

/// <summary>Where a occupancy bundle is in its life cycle.</summary>
public enum OccupancyBundleStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>One occupancy bundle, with what the planners need to act on it.</summary>
public sealed class OccupancyBundle(string code, string title, string team, string ownerId, Dictionary<string, string> attributes, string currency, string costCenter, string grade, double fte, int updatedAt, int quantity, OccupancyBundleStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Team { get; set; } = team;
    public string OwnerId { get; set; } = ownerId;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public string Currency { get; set; } = currency;
    public string CostCenter { get; set; } = costCenter;
    public string Grade { get; set; } = grade;
    public double Fte { get; set; } = fte;
    public int UpdatedAt { get; set; } = updatedAt;
    public int Quantity { get; set; } = quantity;
    public OccupancyBundleStatus Status { get; set; } = status;

    /// <summary>Starts a occupancy bundle in status requested.</summary>
    public static OccupancyBundle NewOccupancyBundle(string code, string title)
    {
        return new OccupancyBundle(
            code,
            title,
            "",
            "",
            new Dictionary<string, string>(),
            "",
            "",
            "",
            0.0,
            0,
            0,
            OccupancyBundleStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case OccupancyBundleStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(OccupancyBundleStatus next)
    {
        switch (Status)
        {
            case OccupancyBundleStatus.Requested:
                return next == OccupancyBundleStatus.Granted || next == OccupancyBundleStatus.Expired;
            case OccupancyBundleStatus.Granted:
                return next == OccupancyBundleStatus.Expired;
            case OccupancyBundleStatus.Revoked:
                switch (next)
                {
                    case OccupancyBundleStatus.Expired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(OccupancyBundleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"occupancy bundle {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        UpdatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
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

    public double EffectiveLoad()
    {
        return Math.Round((double)Quantity - Fte, 2);
    }

    public OccupancyBundle Copy()
    {
        return new OccupancyBundle(
            Code,
            Title,
            Team,
            OwnerId,
            Attributes,
            Currency,
            CostCenter,
            Grade,
            Fte,
            UpdatedAt,
            Quantity,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Team);
    }
}
