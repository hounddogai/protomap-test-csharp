namespace Acme.Workforce.Workplace.Visitor.Estimate;

using Acme.Workforce.Kit.Text;

/// <summary>Where a pending visitor policy is in its life cycle.</summary>
public enum PendingVisitorPolicyStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>A pending visitor policy as the visitor team keeps it.</summary>
public sealed class PendingVisitorPolicy(string code, string label, double maximum, string phone, string reviewer, string category, string managerId, string country, double ceiling, PendingVisitorPolicyStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public double Maximum { get; set; } = maximum;
    public string Phone { get; set; } = phone;
    public string Reviewer { get; set; } = reviewer;
    public string Category { get; set; } = category;
    public string ManagerId { get; set; } = managerId;
    public string Country { get; set; } = country;
    public double Ceiling { get; set; } = ceiling;
    public PendingVisitorPolicyStatus Status { get; set; } = status;

    /// <summary>Starts a pending visitor policy in status idle.</summary>
    public static PendingVisitorPolicy NewPendingVisitorPolicy(string code, string label)
    {
        return new PendingVisitorPolicy(code, label, 0.0, "", "", "", "", "", 0.0, PendingVisitorPolicyStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PendingVisitorPolicyStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PendingVisitorPolicyStatus next)
    {
        switch (Status)
        {
            case PendingVisitorPolicyStatus.Idle:
                switch (next)
                {
                    case PendingVisitorPolicyStatus.Ready:
                    case PendingVisitorPolicyStatus.Draining:
                        return true;
                }
                return false;
            case PendingVisitorPolicyStatus.Warming:
                switch (next)
                {
                    case PendingVisitorPolicyStatus.Draining:
                        return true;
                }
                return false;
            case PendingVisitorPolicyStatus.Ready:
                return next == PendingVisitorPolicyStatus.Draining || next == PendingVisitorPolicyStatus.Stopped;
            case PendingVisitorPolicyStatus.Draining:
                return next == PendingVisitorPolicyStatus.Stopped;
        }
        return false;
    }

    public void MoveTo(PendingVisitorPolicyStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"pending visitor policy {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{"pending visitor policy"} {Code}";
    }

    public double WeightedLoad()
    {
        return Math.Round(Ceiling + Maximum, 2);
    }

    public PendingVisitorPolicy Copy()
    {
        return new PendingVisitorPolicy(
            Code,
            Label,
            Maximum,
            Phone,
            Reviewer,
            Category,
            ManagerId,
            Country,
            Ceiling,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Category);
    }
}
