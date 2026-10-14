namespace Acme.Workforce.Orgdesign.Reorg.Preference;

using Acme.Workforce.Kit.Text;

/// <summary>Where a primary reorg preference is in its life cycle.</summary>
public enum PrimaryReorgPreferenceStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>The record of a primary reorg preference and its current status.</summary>
public sealed class PrimaryReorgPreference(string code, string name, string region, string team, double ratio, string title, string externalRef, string reviewer, string phone, string category, double cost, PrimaryReorgPreferenceStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Region { get; set; } = region;
    public string Team { get; set; } = team;
    public double Ratio { get; set; } = ratio;
    public string Title { get; set; } = title;
    public string ExternalRef { get; set; } = externalRef;
    public string Reviewer { get; set; } = reviewer;
    public string Phone { get; set; } = phone;
    public string Category { get; set; } = category;
    public double Cost { get; set; } = cost;
    public PrimaryReorgPreferenceStatus Status { get; set; } = status;

    /// <summary>Starts a primary reorg preference in status created.</summary>
    public static PrimaryReorgPreference NewPrimaryReorgPreference(string code, string name)
    {
        return new PrimaryReorgPreference(
            code,
            name,
            "",
            "",
            0.0,
            "",
            "",
            "",
            "",
            "",
            0.0,
            PrimaryReorgPreferenceStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PrimaryReorgPreferenceStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PrimaryReorgPreferenceStatus next)
    {
        switch (Status)
        {
            case PrimaryReorgPreferenceStatus.Created:
                switch (next)
                {
                    case PrimaryReorgPreferenceStatus.Validated:
                    case PrimaryReorgPreferenceStatus.Retired:
                        return true;
                }
                return false;
            case PrimaryReorgPreferenceStatus.Validated:
                return next == PrimaryReorgPreferenceStatus.Published || next == PrimaryReorgPreferenceStatus.Retired;
            case PrimaryReorgPreferenceStatus.Published:
                return next == PrimaryReorgPreferenceStatus.Retired;
        }
        return false;
    }

    public void MoveTo(PrimaryReorgPreferenceStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"primary reorg preference {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"primary reorg preference"} {Code}";
    }

    public double AdjustedVolume()
    {
        return Math.Round(Ratio - Cost, 2);
    }

    public PrimaryReorgPreference Copy()
    {
        return new PrimaryReorgPreference(
            Code,
            Name,
            Region,
            Team,
            Ratio,
            Title,
            ExternalRef,
            Reviewer,
            Phone,
            Category,
            Cost,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Region);
    }
}
