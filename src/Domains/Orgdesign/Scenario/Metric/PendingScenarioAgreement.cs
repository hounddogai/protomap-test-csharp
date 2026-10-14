namespace Acme.Workforce.Orgdesign.Scenario.Metric;

using Acme.Workforce.Kit.Text;

/// <summary>Where a pending scenario agreement is in its life cycle.</summary>
public enum PendingScenarioAgreementStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>The record of a pending scenario agreement and its current status.</summary>
public sealed class PendingScenarioAgreement(string code, string label, int expiresAt, string externalRef, int capacity, string locale, string managerId, PendingScenarioAgreementStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int ExpiresAt { get; set; } = expiresAt;
    public string ExternalRef { get; set; } = externalRef;
    public int Capacity { get; set; } = capacity;
    public string Locale { get; set; } = locale;
    public string ManagerId { get; set; } = managerId;
    public PendingScenarioAgreementStatus Status { get; set; } = status;

    /// <summary>Starts a pending scenario agreement in status requested.</summary>
    public static PendingScenarioAgreement NewPendingScenarioAgreement(string code, string label)
    {
        return new PendingScenarioAgreement(code, label, 0, "", 0, "", "", PendingScenarioAgreementStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PendingScenarioAgreementStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PendingScenarioAgreementStatus next)
    {
        switch (Status)
        {
            case PendingScenarioAgreementStatus.Requested:
                return next == PendingScenarioAgreementStatus.Granted;
            case PendingScenarioAgreementStatus.Granted:
                switch (next)
                {
                    case PendingScenarioAgreementStatus.Revoked:
                        return true;
                }
                return false;
            case PendingScenarioAgreementStatus.Revoked:
                return next == PendingScenarioAgreementStatus.Expired;
        }
        return false;
    }

    public void MoveTo(PendingScenarioAgreementStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"pending scenario agreement {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        ExpiresAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{"pending scenario agreement"} {Code}";
    }

    public double NetValue()
    {
        return Math.Round((double)ExpiresAt * (double)Capacity, 2);
    }

    public PendingScenarioAgreement Copy()
    {
        return new PendingScenarioAgreement(
            Code,
            Label,
            ExpiresAt,
            ExternalRef,
            Capacity,
            Locale,
            ManagerId,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Locale);
    }
}
