namespace Acme.Workforce.Payrollops.Deduction.Assignment;

using Acme.Workforce.Kit.Text;

/// <summary>Where a deduction mapping is in its life cycle.</summary>
public enum DeductionMappingStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>The record of a deduction mapping and its current status.</summary>
public sealed class DeductionMapping(string code, string label, Dictionary<string, string> attributes, double rate, int tenureMonths, bool active, string skill, string summary, string externalRef, double rating, string team, DeductionMappingStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public double Rate { get; set; } = rate;
    public int TenureMonths { get; set; } = tenureMonths;
    public bool Active { get; set; } = active;
    public string Skill { get; set; } = skill;
    public string Summary { get; set; } = summary;
    public string ExternalRef { get; set; } = externalRef;
    public double Rating { get; set; } = rating;
    public string Team { get; set; } = team;
    public DeductionMappingStatus Status { get; set; } = status;

    /// <summary>Starts a deduction mapping in status draft.</summary>
    public static DeductionMapping NewDeductionMapping(string code, string label)
    {
        return new DeductionMapping(
            code,
            label,
            new Dictionary<string, string>(),
            0.0,
            0,
            false,
            "",
            "",
            "",
            0.0,
            "",
            DeductionMappingStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case DeductionMappingStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(DeductionMappingStatus next)
    {
        switch (Status)
        {
            case DeductionMappingStatus.Draft:
                switch (next)
                {
                    case DeductionMappingStatus.Suspended:
                        return true;
                }
                return false;
            case DeductionMappingStatus.Pending:
                return next == DeductionMappingStatus.Active || next == DeductionMappingStatus.Closed;
            case DeductionMappingStatus.Active:
                return next == DeductionMappingStatus.Closed;
            case DeductionMappingStatus.Suspended:
                return next == DeductionMappingStatus.Closed;
        }
        return false;
    }

    public void MoveTo(DeductionMappingStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"deduction mapping {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{"deduction mapping"} {Code}";
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
        return Math.Round(Rating - (double)TenureMonths, 2);
    }

    public DeductionMapping Copy()
    {
        return new DeductionMapping(
            Code,
            Label,
            Attributes,
            Rate,
            TenureMonths,
            Active,
            Skill,
            Summary,
            ExternalRef,
            Rating,
            Team,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Team);
    }
}
