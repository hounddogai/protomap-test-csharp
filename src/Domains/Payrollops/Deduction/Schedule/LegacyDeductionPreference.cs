namespace Acme.Workforce.Payrollops.Deduction.Schedule;

using Acme.Workforce.Kit.Text;

/// <summary>Where a legacy deduction preference is in its life cycle.</summary>
public enum LegacyDeductionPreferenceStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>One legacy deduction preference, with what the planners need to act on it.</summary>
public sealed class LegacyDeductionPreference(string code, string name, double weight, string category, double hours, string badge, string checksum, bool pinned, string source, string reviewer, bool archived, LegacyDeductionPreferenceStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public double Weight { get; set; } = weight;
    public string Category { get; set; } = category;
    public double Hours { get; set; } = hours;
    public string Badge { get; set; } = badge;
    public string Checksum { get; set; } = checksum;
    public bool Pinned { get; set; } = pinned;
    public string Source { get; set; } = source;
    public string Reviewer { get; set; } = reviewer;
    public bool Archived { get; set; } = archived;
    public LegacyDeductionPreferenceStatus Status { get; set; } = status;

    /// <summary>Starts a legacy deduction preference in status draft.</summary>
    public static LegacyDeductionPreference NewLegacyDeductionPreference(string code, string name)
    {
        return new LegacyDeductionPreference(
            code,
            name,
            0.0,
            "",
            0.0,
            "",
            "",
            false,
            "",
            "",
            false,
            LegacyDeductionPreferenceStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LegacyDeductionPreferenceStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LegacyDeductionPreferenceStatus next)
    {
        switch (Status)
        {
            case LegacyDeductionPreferenceStatus.Draft:
                return next == LegacyDeductionPreferenceStatus.Pending || next == LegacyDeductionPreferenceStatus.Suspended;
            case LegacyDeductionPreferenceStatus.Pending:
                switch (next)
                {
                    case LegacyDeductionPreferenceStatus.Closed:
                        return true;
                }
                return false;
            case LegacyDeductionPreferenceStatus.Active:
                return next == LegacyDeductionPreferenceStatus.Suspended || next == LegacyDeductionPreferenceStatus.Closed;
            case LegacyDeductionPreferenceStatus.Suspended:
                switch (next)
                {
                    case LegacyDeductionPreferenceStatus.Closed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(LegacyDeductionPreferenceStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"legacy deduction preference {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} / {Name}";
    }

    public double WeightedVolume()
    {
        return Math.Round(Hours + Weight, 2);
    }

    public LegacyDeductionPreference Copy()
    {
        return new LegacyDeductionPreference(
            Code,
            Name,
            Weight,
            Category,
            Hours,
            Badge,
            Checksum,
            Pinned,
            Source,
            Reviewer,
            Archived,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Category);
    }
}
