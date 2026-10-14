namespace Acme.Workforce.Workplace.FloorPlan.Agreement;

using Acme.Workforce.Kit.Text;

/// <summary>Where a floor plan agreement is in its life cycle.</summary>
public enum FloorPlanAgreementStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>The record of a floor plan agreement and its current status.</summary>
public sealed class FloorPlanAgreement(string code, string name, double amount, string channel, int revision, double score, string contactEmail, string label, string locale, int updatedAt, FloorPlanAgreementStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public double Amount { get; set; } = amount;
    public string Channel { get; set; } = channel;
    public int Revision { get; set; } = revision;
    public double Score { get; set; } = score;
    public string ContactEmail { get; set; } = contactEmail;
    public string Label { get; set; } = label;
    public string Locale { get; set; } = locale;
    public int UpdatedAt { get; set; } = updatedAt;
    public FloorPlanAgreementStatus Status { get; set; } = status;

    /// <summary>Starts a floor plan agreement in status planned.</summary>
    public static FloorPlanAgreement NewFloorPlanAgreement(string code, string name)
    {
        return new FloorPlanAgreement(code, name, 0.0, "", 0, 0.0, "", "", "", 0, FloorPlanAgreementStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case FloorPlanAgreementStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(FloorPlanAgreementStatus next)
    {
        switch (Status)
        {
            case FloorPlanAgreementStatus.Planned:
                return next == FloorPlanAgreementStatus.Scheduled || next == FloorPlanAgreementStatus.InProgress;
            case FloorPlanAgreementStatus.Scheduled:
                switch (next)
                {
                    case FloorPlanAgreementStatus.Cancelled:
                        return true;
                }
                return false;
            case FloorPlanAgreementStatus.InProgress:
                switch (next)
                {
                    case FloorPlanAgreementStatus.Completed:
                    case FloorPlanAgreementStatus.Cancelled:
                        return true;
                }
                return false;
            case FloorPlanAgreementStatus.Completed:
                return next == FloorPlanAgreementStatus.Cancelled;
        }
        return false;
    }

    public void MoveTo(FloorPlanAgreementStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"floor plan agreement {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        UpdatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        Revision += 1;
    }

    public string DisplayLabel()
    {
        return $"{"floor plan agreement"} {Code}";
    }

    public double AdjustedValue()
    {
        return Math.Round(Amount + (double)Revision, 2);
    }

    public FloorPlanAgreement Copy()
    {
        return new FloorPlanAgreement(
            Code,
            Name,
            Amount,
            Channel,
            Revision,
            Score,
            ContactEmail,
            Label,
            Locale,
            UpdatedAt,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Channel);
    }
}
