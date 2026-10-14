namespace Acme.Workforce.Talent.Nomination.NominationPolicy;

using Acme.Workforce.Kit.Text;

/// <summary>Where a regional nomination policy is in its life cycle.</summary>
public enum RegionalNominationPolicyStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>A regional nomination policy. Its code is unique within a tenant.</summary>
public sealed class RegionalNominationPolicy(string code, string name, double fte, int leaveDays, string checksum, double budget, double discount, string salaryBand, string phone, string category, string contactEmail, RegionalNominationPolicyStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public double Fte { get; set; } = fte;
    public int LeaveDays { get; set; } = leaveDays;
    public string Checksum { get; set; } = checksum;
    public double Budget { get; set; } = budget;
    public double Discount { get; set; } = discount;
    public string SalaryBand { get; set; } = salaryBand;
    public string Phone { get; set; } = phone;
    public string Category { get; set; } = category;
    public string ContactEmail { get; set; } = contactEmail;
    public RegionalNominationPolicyStatus Status { get; set; } = status;

    /// <summary>Starts a regional nomination policy in status idle.</summary>
    public static RegionalNominationPolicy NewRegionalNominationPolicy(string code, string name)
    {
        return new RegionalNominationPolicy(
            code,
            name,
            0.0,
            0,
            "",
            0.0,
            0.0,
            "",
            "",
            "",
            "",
            RegionalNominationPolicyStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RegionalNominationPolicyStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RegionalNominationPolicyStatus next)
    {
        switch (Status)
        {
            case RegionalNominationPolicyStatus.Idle:
                switch (next)
                {
                    case RegionalNominationPolicyStatus.Draining:
                    case RegionalNominationPolicyStatus.Stopped:
                        return true;
                }
                return false;
            case RegionalNominationPolicyStatus.Warming:
                switch (next)
                {
                    case RegionalNominationPolicyStatus.Ready:
                        return true;
                }
                return false;
            case RegionalNominationPolicyStatus.Ready:
                return next == RegionalNominationPolicyStatus.Stopped;
            case RegionalNominationPolicyStatus.Draining:
                return next == RegionalNominationPolicyStatus.Stopped;
        }
        return false;
    }

    public void MoveTo(RegionalNominationPolicyStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"regional nomination policy {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"regional nomination policy"} {Code}";
    }

    public double EffectiveTotal()
    {
        return Math.Round((double)LeaveDays * Fte, 2);
    }

    public RegionalNominationPolicy Copy()
    {
        return new RegionalNominationPolicy(
            Code,
            Name,
            Fte,
            LeaveDays,
            Checksum,
            Budget,
            Discount,
            SalaryBand,
            Phone,
            Category,
            ContactEmail,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Category);
    }
}
