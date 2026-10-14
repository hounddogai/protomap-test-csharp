namespace Acme.Workforce.Payrollops.Garnishment.Tier;

using Acme.Workforce.Kit.Text;

/// <summary>Where a garnishment baseline is in its life cycle.</summary>
public enum GarnishmentBaselineStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>The record of a garnishment baseline and its current status.</summary>
public sealed class GarnishmentBaseline(string code, string label, double rate, int version, int startsAt, string notes, List<string> tags, bool approved, double ceiling, int limit, string timezone, GarnishmentBaselineStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public double Rate { get; set; } = rate;
    public int Version { get; set; } = version;
    public int StartsAt { get; set; } = startsAt;
    public string Notes { get; set; } = notes;
    public List<string> Tags { get; set; } = tags;
    public bool Approved { get; set; } = approved;
    public double Ceiling { get; set; } = ceiling;
    public int Limit { get; set; } = limit;
    public string Timezone { get; set; } = timezone;
    public GarnishmentBaselineStatus Status { get; set; } = status;

    /// <summary>Starts a garnishment baseline in status created.</summary>
    public static GarnishmentBaseline NewGarnishmentBaseline(string code, string label)
    {
        return new GarnishmentBaseline(
            code,
            label,
            0.0,
            0,
            0,
            "",
            new List<string>(),
            false,
            0.0,
            0,
            "",
            GarnishmentBaselineStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case GarnishmentBaselineStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(GarnishmentBaselineStatus next)
    {
        switch (Status)
        {
            case GarnishmentBaselineStatus.Created:
                switch (next)
                {
                    case GarnishmentBaselineStatus.Published:
                        return true;
                }
                return false;
            case GarnishmentBaselineStatus.Validated:
                switch (next)
                {
                    case GarnishmentBaselineStatus.Published:
                    case GarnishmentBaselineStatus.Retired:
                        return true;
                }
                return false;
            case GarnishmentBaselineStatus.Published:
                return next == GarnishmentBaselineStatus.Retired;
        }
        return false;
    }

    public void MoveTo(GarnishmentBaselineStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"garnishment baseline {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        StartsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        Version += 1;
    }

    public string DisplayLabel()
    {
        return $"{"garnishment baseline"} {Code}";
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

    public double EffectiveExposure()
    {
        return Math.Round((double)Version * Rate, 2);
    }

    public GarnishmentBaseline Copy()
    {
        return new GarnishmentBaseline(
            Code,
            Label,
            Rate,
            Version,
            StartsAt,
            Notes,
            Tags,
            Approved,
            Ceiling,
            Limit,
            Timezone,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
