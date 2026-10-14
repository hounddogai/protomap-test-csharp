namespace Acme.Workforce.Compliance.TrainingDue.CappedDueVariance;

using Acme.Workforce.Kit.Text;

/// <summary>Where a adaptive training due claim is in its life cycle.</summary>
public enum AdaptiveTrainingDueClaimStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>A adaptive training due claim. Its code is unique within a tenant.</summary>
public sealed class AdaptiveTrainingDueClaim(string code, string title, int batchSize, double weight, string label, string phone, int revision, string externalRef, double minimum, string contactEmail, int leaveDays, AdaptiveTrainingDueClaimStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int BatchSize { get; set; } = batchSize;
    public double Weight { get; set; } = weight;
    public string Label { get; set; } = label;
    public string Phone { get; set; } = phone;
    public int Revision { get; set; } = revision;
    public string ExternalRef { get; set; } = externalRef;
    public double Minimum { get; set; } = minimum;
    public string ContactEmail { get; set; } = contactEmail;
    public int LeaveDays { get; set; } = leaveDays;
    public AdaptiveTrainingDueClaimStatus Status { get; set; } = status;

    /// <summary>Starts a adaptive training due claim in status created.</summary>
    public static AdaptiveTrainingDueClaim NewAdaptiveTrainingDueClaim(string code, string title)
    {
        return new AdaptiveTrainingDueClaim(
            code,
            title,
            0,
            0.0,
            "",
            "",
            0,
            "",
            0.0,
            "",
            0,
            AdaptiveTrainingDueClaimStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AdaptiveTrainingDueClaimStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AdaptiveTrainingDueClaimStatus next)
    {
        switch (Status)
        {
            case AdaptiveTrainingDueClaimStatus.Created:
                switch (next)
                {
                    case AdaptiveTrainingDueClaimStatus.Validated:
                    case AdaptiveTrainingDueClaimStatus.Published:
                        return true;
                }
                return false;
            case AdaptiveTrainingDueClaimStatus.Validated:
                switch (next)
                {
                    case AdaptiveTrainingDueClaimStatus.Retired:
                        return true;
                }
                return false;
            case AdaptiveTrainingDueClaimStatus.Published:
                return next == AdaptiveTrainingDueClaimStatus.Retired;
        }
        return false;
    }

    public void MoveTo(AdaptiveTrainingDueClaimStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"adaptive training due claim {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Revision += 1;
    }

    public string DisplayLabel()
    {
        return $"{Code} ({Title})";
    }

    public double EffectiveLoad()
    {
        return Math.Round(Weight + (double)BatchSize, 2);
    }

    public AdaptiveTrainingDueClaim Copy()
    {
        return new AdaptiveTrainingDueClaim(
            Code,
            Title,
            BatchSize,
            Weight,
            Label,
            Phone,
            Revision,
            ExternalRef,
            Minimum,
            ContactEmail,
            LeaveDays,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
