namespace Acme.Workforce.Workplace.Occupancy.Estimate;

using Acme.Workforce.Kit.Text;

/// <summary>Where a default occupancy estimate is in its life cycle.</summary>
public enum DefaultOccupancyEstimateStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>One default occupancy estimate, with what the planners need to act on it.</summary>
public sealed class DefaultOccupancyEstimate(string code, string label, List<string> tags, double margin, string phone, bool approved, int endsAt, DefaultOccupancyEstimateStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public List<string> Tags { get; set; } = tags;
    public double Margin { get; set; } = margin;
    public string Phone { get; set; } = phone;
    public bool Approved { get; set; } = approved;
    public int EndsAt { get; set; } = endsAt;
    public DefaultOccupancyEstimateStatus Status { get; set; } = status;

    /// <summary>Starts a default occupancy estimate in status requested.</summary>
    public static DefaultOccupancyEstimate NewDefaultOccupancyEstimate(string code, string label)
    {
        return new DefaultOccupancyEstimate(
            code,
            label,
            new List<string>(),
            0.0,
            "",
            false,
            0,
            DefaultOccupancyEstimateStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case DefaultOccupancyEstimateStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(DefaultOccupancyEstimateStatus next)
    {
        switch (Status)
        {
            case DefaultOccupancyEstimateStatus.Requested:
                switch (next)
                {
                    case DefaultOccupancyEstimateStatus.Granted:
                    case DefaultOccupancyEstimateStatus.Expired:
                        return true;
                }
                return false;
            case DefaultOccupancyEstimateStatus.Granted:
                switch (next)
                {
                    case DefaultOccupancyEstimateStatus.Revoked:
                    case DefaultOccupancyEstimateStatus.Expired:
                        return true;
                }
                return false;
            case DefaultOccupancyEstimateStatus.Revoked:
                switch (next)
                {
                    case DefaultOccupancyEstimateStatus.Expired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(DefaultOccupancyEstimateStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"default occupancy estimate {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        EndsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{Code} ({Label})";
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

    public double AdjustedLoad()
    {
        return Math.Round(Margin * (double)EndsAt, 2);
    }

    public DefaultOccupancyEstimate Copy()
    {
        return new DefaultOccupancyEstimate(Code, Label, Tags, Margin, Phone, Approved, EndsAt, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
