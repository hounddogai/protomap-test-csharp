namespace Acme.Workforce.Absence.ReturnPlan.Tier;

using Acme.Workforce.Kit.Text;

/// <summary>Where a default return plan tier is in its life cycle.</summary>
public enum DefaultReturnPlanTierStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>One default return plan tier, with what the planners need to act on it.</summary>
public sealed class DefaultReturnPlanTier(string code, string label, int durationMinutes, int windowDays, string channel, bool pinned, Dictionary<string, string> attributes, int createdAt, DefaultReturnPlanTierStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int DurationMinutes { get; set; } = durationMinutes;
    public int WindowDays { get; set; } = windowDays;
    public string Channel { get; set; } = channel;
    public bool Pinned { get; set; } = pinned;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public int CreatedAt { get; set; } = createdAt;
    public DefaultReturnPlanTierStatus Status { get; set; } = status;

    /// <summary>Starts a default return plan tier in status created.</summary>
    public static DefaultReturnPlanTier NewDefaultReturnPlanTier(string code, string label)
    {
        return new DefaultReturnPlanTier(
            code,
            label,
            0,
            0,
            "",
            false,
            new Dictionary<string, string>(),
            0,
            DefaultReturnPlanTierStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case DefaultReturnPlanTierStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(DefaultReturnPlanTierStatus next)
    {
        switch (Status)
        {
            case DefaultReturnPlanTierStatus.Created:
                return next == DefaultReturnPlanTierStatus.Validated || next == DefaultReturnPlanTierStatus.Retired;
            case DefaultReturnPlanTierStatus.Validated:
                return next == DefaultReturnPlanTierStatus.Published;
            case DefaultReturnPlanTierStatus.Published:
                switch (next)
                {
                    case DefaultReturnPlanTierStatus.Retired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(DefaultReturnPlanTierStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"default return plan tier {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        CreatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{Code} [{Label}]";
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
        return Math.Round((double)DurationMinutes + (double)CreatedAt, 2);
    }

    public DefaultReturnPlanTier Copy()
    {
        return new DefaultReturnPlanTier(
            Code,
            Label,
            DurationMinutes,
            WindowDays,
            Channel,
            Pinned,
            Attributes,
            CreatedAt,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Channel);
    }
}
