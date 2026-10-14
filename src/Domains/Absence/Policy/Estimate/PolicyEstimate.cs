namespace Acme.Workforce.Absence.Policy.Estimate;

using Acme.Workforce.Kit.Text;

/// <summary>Where a policy estimate is in its life cycle.</summary>
public enum PolicyEstimateStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>The record of a policy estimate and its current status.</summary>
public sealed class PolicyEstimate(string code, string title, bool pinned, string jobCode, double budget, int limit, string contactEmail, double minimum, string skill, double discount, double hours, PolicyEstimateStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public bool Pinned { get; set; } = pinned;
    public string JobCode { get; set; } = jobCode;
    public double Budget { get; set; } = budget;
    public int Limit { get; set; } = limit;
    public string ContactEmail { get; set; } = contactEmail;
    public double Minimum { get; set; } = minimum;
    public string Skill { get; set; } = skill;
    public double Discount { get; set; } = discount;
    public double Hours { get; set; } = hours;
    public PolicyEstimateStatus Status { get; set; } = status;

    /// <summary>Starts a policy estimate in status proposed.</summary>
    public static PolicyEstimate NewPolicyEstimate(string code, string title)
    {
        return new PolicyEstimate(
            code,
            title,
            false,
            "",
            0.0,
            0,
            "",
            0.0,
            "",
            0.0,
            0.0,
            PolicyEstimateStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PolicyEstimateStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PolicyEstimateStatus next)
    {
        switch (Status)
        {
            case PolicyEstimateStatus.Proposed:
                switch (next)
                {
                    case PolicyEstimateStatus.Approved:
                    case PolicyEstimateStatus.Withdrawn:
                        return true;
                }
                return false;
            case PolicyEstimateStatus.Approved:
                return next == PolicyEstimateStatus.Rejected || next == PolicyEstimateStatus.Withdrawn;
            case PolicyEstimateStatus.Rejected:
                return next == PolicyEstimateStatus.Withdrawn;
        }
        return false;
    }

    public void MoveTo(PolicyEstimateStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"policy estimate {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"policy estimate"} {Code}";
    }

    public double ProjectedTotal()
    {
        return Math.Round(Minimum * Budget, 2);
    }

    public PolicyEstimate Copy()
    {
        return new PolicyEstimate(
            Code,
            Title,
            Pinned,
            JobCode,
            Budget,
            Limit,
            ContactEmail,
            Minimum,
            Skill,
            Discount,
            Hours,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
