namespace Acme.Workforce.Orgdesign.Headcount.Agreement;

using Acme.Workforce.Kit.Text;

/// <summary>Where a headcount agreement is in its life cycle.</summary>
public enum HeadcountAgreementStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>The record of a headcount agreement and its current status.</summary>
public sealed class HeadcountAgreement(string code, string label, double floorValue, int startsAt, string externalRef, string contactEmail, int expiresAt, double maximum, string managerId, double ceiling, string ownerId, HeadcountAgreementStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public double FloorValue { get; set; } = floorValue;
    public int StartsAt { get; set; } = startsAt;
    public string ExternalRef { get; set; } = externalRef;
    public string ContactEmail { get; set; } = contactEmail;
    public int ExpiresAt { get; set; } = expiresAt;
    public double Maximum { get; set; } = maximum;
    public string ManagerId { get; set; } = managerId;
    public double Ceiling { get; set; } = ceiling;
    public string OwnerId { get; set; } = ownerId;
    public HeadcountAgreementStatus Status { get; set; } = status;

    /// <summary>Starts a headcount agreement in status idle.</summary>
    public static HeadcountAgreement NewHeadcountAgreement(string code, string label)
    {
        return new HeadcountAgreement(
            code,
            label,
            0.0,
            0,
            "",
            "",
            0,
            0.0,
            "",
            0.0,
            "",
            HeadcountAgreementStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case HeadcountAgreementStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(HeadcountAgreementStatus next)
    {
        switch (Status)
        {
            case HeadcountAgreementStatus.Idle:
                switch (next)
                {
                    case HeadcountAgreementStatus.Draining:
                        return true;
                }
                return false;
            case HeadcountAgreementStatus.Warming:
                return next == HeadcountAgreementStatus.Ready;
            case HeadcountAgreementStatus.Ready:
                switch (next)
                {
                    case HeadcountAgreementStatus.Draining:
                    case HeadcountAgreementStatus.Stopped:
                        return true;
                }
                return false;
            case HeadcountAgreementStatus.Draining:
                switch (next)
                {
                    case HeadcountAgreementStatus.Stopped:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(HeadcountAgreementStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"headcount agreement {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        StartsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{"headcount agreement"} {Code}";
    }

    public double NetExposure()
    {
        return Math.Round(Ceiling + Maximum, 2);
    }

    public HeadcountAgreement Copy()
    {
        return new HeadcountAgreement(
            Code,
            Label,
            FloorValue,
            StartsAt,
            ExternalRef,
            ContactEmail,
            ExpiresAt,
            Maximum,
            ManagerId,
            Ceiling,
            OwnerId,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
