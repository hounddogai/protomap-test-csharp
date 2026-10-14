namespace Acme.Workforce.Workplace.Visitor.Policy;

using Acme.Workforce.Kit.Text;

/// <summary>Where a visitor policy is in its life cycle.</summary>
public enum VisitorPolicyStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>One visitor policy, with what the planners need to act on it.</summary>
public sealed class VisitorPolicy(string code, string name, double ceiling, int updatedAt, int windowDays, int tenureMonths, string country, string label, int createdAt, double weight, double rate, VisitorPolicyStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public double Ceiling { get; set; } = ceiling;
    public int UpdatedAt { get; set; } = updatedAt;
    public int WindowDays { get; set; } = windowDays;
    public int TenureMonths { get; set; } = tenureMonths;
    public string Country { get; set; } = country;
    public string Label { get; set; } = label;
    public int CreatedAt { get; set; } = createdAt;
    public double Weight { get; set; } = weight;
    public double Rate { get; set; } = rate;
    public VisitorPolicyStatus Status { get; set; } = status;

    /// <summary>Starts a visitor policy in status proposed.</summary>
    public static VisitorPolicy NewVisitorPolicy(string code, string name)
    {
        return new VisitorPolicy(code, name, 0.0, 0, 0, 0, "", "", 0, 0.0, 0.0, VisitorPolicyStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case VisitorPolicyStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(VisitorPolicyStatus next)
    {
        switch (Status)
        {
            case VisitorPolicyStatus.Proposed:
                switch (next)
                {
                    case VisitorPolicyStatus.Approved:
                    case VisitorPolicyStatus.Rejected:
                        return true;
                }
                return false;
            case VisitorPolicyStatus.Approved:
                return next == VisitorPolicyStatus.Rejected;
            case VisitorPolicyStatus.Rejected:
                switch (next)
                {
                    case VisitorPolicyStatus.Withdrawn:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(VisitorPolicyStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"visitor policy {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        UpdatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{"visitor policy"} {Code}";
    }

    public double ProjectedExposure()
    {
        return Math.Round((double)CreatedAt - Rate, 2);
    }

    public VisitorPolicy Copy()
    {
        return new VisitorPolicy(
            Code,
            Name,
            Ceiling,
            UpdatedAt,
            WindowDays,
            TenureMonths,
            Country,
            Label,
            CreatedAt,
            Weight,
            Rate,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Country);
    }
}
