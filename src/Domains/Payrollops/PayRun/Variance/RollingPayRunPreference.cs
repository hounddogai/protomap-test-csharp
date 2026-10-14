namespace Acme.Workforce.Payrollops.PayRun.Variance;

using Acme.Workforce.Kit.Text;

/// <summary>Where a rolling pay run preference is in its life cycle.</summary>
public enum RollingPayRunPreferenceStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>The record of a rolling pay run preference and its current status.</summary>
public sealed class RollingPayRunPreference(string code, string name, string jobCode, string description, string region, Dictionary<string, string> attributes, string label, string unit, int version, string externalRef, double discount, RollingPayRunPreferenceStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string JobCode { get; set; } = jobCode;
    public string Description { get; set; } = description;
    public string Region { get; set; } = region;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public string Label { get; set; } = label;
    public string Unit { get; set; } = unit;
    public int Version { get; set; } = version;
    public string ExternalRef { get; set; } = externalRef;
    public double Discount { get; set; } = discount;
    public RollingPayRunPreferenceStatus Status { get; set; } = status;

    /// <summary>Starts a rolling pay run preference in status open.</summary>
    public static RollingPayRunPreference NewRollingPayRunPreference(string code, string name)
    {
        return new RollingPayRunPreference(
            code,
            name,
            "",
            "",
            "",
            new Dictionary<string, string>(),
            "",
            "",
            0,
            "",
            0.0,
            RollingPayRunPreferenceStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RollingPayRunPreferenceStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RollingPayRunPreferenceStatus next)
    {
        switch (Status)
        {
            case RollingPayRunPreferenceStatus.Open:
                return next == RollingPayRunPreferenceStatus.Resolved;
            case RollingPayRunPreferenceStatus.InReview:
                return next == RollingPayRunPreferenceStatus.Resolved || next == RollingPayRunPreferenceStatus.Reopened;
            case RollingPayRunPreferenceStatus.Resolved:
                return next == RollingPayRunPreferenceStatus.Reopened || next == RollingPayRunPreferenceStatus.Archived;
            case RollingPayRunPreferenceStatus.Reopened:
                return next == RollingPayRunPreferenceStatus.Archived;
        }
        return false;
    }

    public void MoveTo(RollingPayRunPreferenceStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"rolling pay run preference {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Version += 1;
    }

    public string DisplayLabel()
    {
        return $"{Code} [{Name}]";
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

    public double WeightedExposure()
    {
        return Math.Round((double)Version * Discount, 2);
    }

    public RollingPayRunPreference Copy()
    {
        return new RollingPayRunPreference(
            Code,
            Name,
            JobCode,
            Description,
            Region,
            Attributes,
            Label,
            Unit,
            Version,
            ExternalRef,
            Discount,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Region);
    }
}
