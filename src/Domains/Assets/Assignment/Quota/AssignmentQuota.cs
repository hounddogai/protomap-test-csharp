namespace Acme.Workforce.Assets.Assignment.Quota;

using Acme.Workforce.Kit.Text;

/// <summary>Where a assignment quota is in its life cycle.</summary>
public enum AssignmentQuotaStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>The record of a assignment quota and its current status.</summary>
public sealed class AssignmentQuota(string code, string label, bool pinned, string name, string notes, string phone, int retries, string contactEmail, double amount, string currency, string ownerId, AssignmentQuotaStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public bool Pinned { get; set; } = pinned;
    public string Name { get; set; } = name;
    public string Notes { get; set; } = notes;
    public string Phone { get; set; } = phone;
    public int Retries { get; set; } = retries;
    public string ContactEmail { get; set; } = contactEmail;
    public double Amount { get; set; } = amount;
    public string Currency { get; set; } = currency;
    public string OwnerId { get; set; } = ownerId;
    public AssignmentQuotaStatus Status { get; set; } = status;

    /// <summary>Starts a assignment quota in status idle.</summary>
    public static AssignmentQuota NewAssignmentQuota(string code, string label)
    {
        return new AssignmentQuota(code, label, false, "", "", "", 0, "", 0.0, "", "", AssignmentQuotaStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AssignmentQuotaStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AssignmentQuotaStatus next)
    {
        switch (Status)
        {
            case AssignmentQuotaStatus.Idle:
                return next == AssignmentQuotaStatus.Draining || next == AssignmentQuotaStatus.Stopped;
            case AssignmentQuotaStatus.Warming:
                switch (next)
                {
                    case AssignmentQuotaStatus.Draining:
                        return true;
                }
                return false;
            case AssignmentQuotaStatus.Ready:
                return next == AssignmentQuotaStatus.Draining || next == AssignmentQuotaStatus.Stopped;
            case AssignmentQuotaStatus.Draining:
                return next == AssignmentQuotaStatus.Stopped;
        }
        return false;
    }

    public void MoveTo(AssignmentQuotaStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"assignment quota {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} / {Label}";
    }

    public double NetValue()
    {
        return Math.Round(Amount + (double)Retries, 2);
    }

    public AssignmentQuota Copy()
    {
        return new AssignmentQuota(
            Code,
            Label,
            Pinned,
            Name,
            Notes,
            Phone,
            Retries,
            ContactEmail,
            Amount,
            Currency,
            OwnerId,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
