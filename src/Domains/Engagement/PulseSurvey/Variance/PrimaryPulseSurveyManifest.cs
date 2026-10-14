namespace Acme.Workforce.Engagement.PulseSurvey.Variance;

using Acme.Workforce.Kit.Text;

/// <summary>Where a primary pulse survey manifest is in its life cycle.</summary>
public enum PrimaryPulseSurveyManifestStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>A primary pulse survey manifest as the pulse survey team keeps it.</summary>
public sealed class PrimaryPulseSurveyManifest(string code, string title, string employeeId, double fte, string channel, double ceiling, PrimaryPulseSurveyManifestStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string EmployeeId { get; set; } = employeeId;
    public double Fte { get; set; } = fte;
    public string Channel { get; set; } = channel;
    public double Ceiling { get; set; } = ceiling;
    public PrimaryPulseSurveyManifestStatus Status { get; set; } = status;

    /// <summary>Starts a primary pulse survey manifest in status created.</summary>
    public static PrimaryPulseSurveyManifest NewPrimaryPulseSurveyManifest(string code, string title)
    {
        return new PrimaryPulseSurveyManifest(
            code,
            title,
            "",
            0.0,
            "",
            0.0,
            PrimaryPulseSurveyManifestStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PrimaryPulseSurveyManifestStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PrimaryPulseSurveyManifestStatus next)
    {
        switch (Status)
        {
            case PrimaryPulseSurveyManifestStatus.Created:
                switch (next)
                {
                    case PrimaryPulseSurveyManifestStatus.Validated:
                    case PrimaryPulseSurveyManifestStatus.Published:
                        return true;
                }
                return false;
            case PrimaryPulseSurveyManifestStatus.Validated:
                return next == PrimaryPulseSurveyManifestStatus.Published || next == PrimaryPulseSurveyManifestStatus.Retired;
            case PrimaryPulseSurveyManifestStatus.Published:
                return next == PrimaryPulseSurveyManifestStatus.Retired;
        }
        return false;
    }

    public void MoveTo(PrimaryPulseSurveyManifestStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"primary pulse survey manifest {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} / {Title}";
    }

    public double AdjustedValue()
    {
        return Math.Round(Fte + Ceiling, 2);
    }

    public PrimaryPulseSurveyManifest Copy()
    {
        return new PrimaryPulseSurveyManifest(Code, Title, EmployeeId, Fte, Channel, Ceiling, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Channel);
    }
}
