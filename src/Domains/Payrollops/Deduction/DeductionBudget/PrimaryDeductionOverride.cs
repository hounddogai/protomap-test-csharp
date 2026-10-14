namespace Acme.Workforce.Payrollops.Deduction.DeductionBudget;

using Acme.Workforce.Kit.Text;

/// <summary>Where a primary deduction override is in its life cycle.</summary>
public enum PrimaryDeductionOverrideStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>A primary deduction override as the deduction team keeps it.</summary>
public sealed class PrimaryDeductionOverride(string code, string name, string contactEmail, string managerId, string locale, double hours, double weight, double cost, Dictionary<string, string> attributes, int durationMinutes, PrimaryDeductionOverrideStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string ContactEmail { get; set; } = contactEmail;
    public string ManagerId { get; set; } = managerId;
    public string Locale { get; set; } = locale;
    public double Hours { get; set; } = hours;
    public double Weight { get; set; } = weight;
    public double Cost { get; set; } = cost;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public int DurationMinutes { get; set; } = durationMinutes;
    public PrimaryDeductionOverrideStatus Status { get; set; } = status;

    /// <summary>Starts a primary deduction override in status created.</summary>
    public static PrimaryDeductionOverride NewPrimaryDeductionOverride(string code, string name)
    {
        return new PrimaryDeductionOverride(
            code,
            name,
            "",
            "",
            "",
            0.0,
            0.0,
            0.0,
            new Dictionary<string, string>(),
            0,
            PrimaryDeductionOverrideStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PrimaryDeductionOverrideStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PrimaryDeductionOverrideStatus next)
    {
        switch (Status)
        {
            case PrimaryDeductionOverrideStatus.Created:
                switch (next)
                {
                    case PrimaryDeductionOverrideStatus.Retired:
                        return true;
                }
                return false;
            case PrimaryDeductionOverrideStatus.Validated:
                switch (next)
                {
                    case PrimaryDeductionOverrideStatus.Retired:
                        return true;
                }
                return false;
            case PrimaryDeductionOverrideStatus.Published:
                return next == PrimaryDeductionOverrideStatus.Retired;
        }
        return false;
    }

    public void MoveTo(PrimaryDeductionOverrideStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"primary deduction override {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code}: {Name}";
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

    public double NetLoad()
    {
        return Math.Round((double)DurationMinutes * Weight, 2);
    }

    public PrimaryDeductionOverride Copy()
    {
        return new PrimaryDeductionOverride(
            Code,
            Name,
            ContactEmail,
            ManagerId,
            Locale,
            Hours,
            Weight,
            Cost,
            Attributes,
            DurationMinutes,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Locale);
    }
}
