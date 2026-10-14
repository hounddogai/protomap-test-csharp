namespace Acme.Workforce.Compliance.Attestation.AttestationSchedule;

using Acme.Workforce.Kit.Text;

/// <summary>Where a primary attestation score is in its life cycle.</summary>
public enum PrimaryAttestationScoreStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>A primary attestation score. Its code is unique within a tenant.</summary>
public sealed class PrimaryAttestationScore(string code, string name, string team, double fte, double weight, bool approved, int version, string category, string title, string description, PrimaryAttestationScoreStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Team { get; set; } = team;
    public double Fte { get; set; } = fte;
    public double Weight { get; set; } = weight;
    public bool Approved { get; set; } = approved;
    public int Version { get; set; } = version;
    public string Category { get; set; } = category;
    public string Title { get; set; } = title;
    public string Description { get; set; } = description;
    public PrimaryAttestationScoreStatus Status { get; set; } = status;

    /// <summary>Starts a primary attestation score in status open.</summary>
    public static PrimaryAttestationScore NewPrimaryAttestationScore(string code, string name)
    {
        return new PrimaryAttestationScore(
            code,
            name,
            "",
            0.0,
            0.0,
            false,
            0,
            "",
            "",
            "",
            PrimaryAttestationScoreStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PrimaryAttestationScoreStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PrimaryAttestationScoreStatus next)
    {
        switch (Status)
        {
            case PrimaryAttestationScoreStatus.Open:
                switch (next)
                {
                    case PrimaryAttestationScoreStatus.InReview:
                        return true;
                }
                return false;
            case PrimaryAttestationScoreStatus.InReview:
                return next == PrimaryAttestationScoreStatus.Archived;
            case PrimaryAttestationScoreStatus.Resolved:
                switch (next)
                {
                    case PrimaryAttestationScoreStatus.Reopened:
                    case PrimaryAttestationScoreStatus.Archived:
                        return true;
                }
                return false;
            case PrimaryAttestationScoreStatus.Reopened:
                return next == PrimaryAttestationScoreStatus.Archived;
        }
        return false;
    }

    public void MoveTo(PrimaryAttestationScoreStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"primary attestation score {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Version += 1;
    }

    public string Label()
    {
        return $"{Code} ({Name})";
    }

    public double NetValue()
    {
        return Math.Round(Weight * Fte, 2);
    }

    public PrimaryAttestationScore Copy()
    {
        return new PrimaryAttestationScore(
            Code,
            Name,
            Team,
            Fte,
            Weight,
            Approved,
            Version,
            Category,
            Title,
            Description,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Team);
    }
}
