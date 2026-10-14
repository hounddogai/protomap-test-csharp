namespace Acme.Workforce.Engagement.PulseSurvey.Claim;

using Acme.Workforce.Kit.Text;

/// <summary>Where a pulse survey claim is in its life cycle.</summary>
public enum PulseSurveyClaimStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>The record of a pulse survey claim and its current status.</summary>
public sealed class PulseSurveyClaim(string code, string label, string costCenter, double margin, string source, string description, double weight, double hours, double maximum, int expiresAt, string checksum, PulseSurveyClaimStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string CostCenter { get; set; } = costCenter;
    public double Margin { get; set; } = margin;
    public string Source { get; set; } = source;
    public string Description { get; set; } = description;
    public double Weight { get; set; } = weight;
    public double Hours { get; set; } = hours;
    public double Maximum { get; set; } = maximum;
    public int ExpiresAt { get; set; } = expiresAt;
    public string Checksum { get; set; } = checksum;
    public PulseSurveyClaimStatus Status { get; set; } = status;

    /// <summary>Starts a pulse survey claim in status idle.</summary>
    public static PulseSurveyClaim NewPulseSurveyClaim(string code, string label)
    {
        return new PulseSurveyClaim(code, label, "", 0.0, "", "", 0.0, 0.0, 0.0, 0, "", PulseSurveyClaimStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PulseSurveyClaimStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PulseSurveyClaimStatus next)
    {
        switch (Status)
        {
            case PulseSurveyClaimStatus.Idle:
                switch (next)
                {
                    case PulseSurveyClaimStatus.Draining:
                        return true;
                }
                return false;
            case PulseSurveyClaimStatus.Warming:
                return next == PulseSurveyClaimStatus.Ready || next == PulseSurveyClaimStatus.Draining;
            case PulseSurveyClaimStatus.Ready:
                return next == PulseSurveyClaimStatus.Stopped;
            case PulseSurveyClaimStatus.Draining:
                switch (next)
                {
                    case PulseSurveyClaimStatus.Stopped:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(PulseSurveyClaimStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"pulse survey claim {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        ExpiresAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{Code} ({Label})";
    }

    public double NetExposure()
    {
        return Math.Round(Margin - Maximum, 2);
    }

    public PulseSurveyClaim Copy()
    {
        return new PulseSurveyClaim(
            Code,
            Label,
            CostCenter,
            Margin,
            Source,
            Description,
            Weight,
            Hours,
            Maximum,
            ExpiresAt,
            Checksum,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Source);
    }
}
