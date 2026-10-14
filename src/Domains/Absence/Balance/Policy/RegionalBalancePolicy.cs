namespace Acme.Workforce.Absence.Balance.Policy;

using Acme.Workforce.Kit.Text;

/// <summary>Where a regional balance policy is in its life cycle.</summary>
public enum RegionalBalancePolicyStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>A regional balance policy as the balance team keeps it.</summary>
public sealed class RegionalBalancePolicy(string code, string title, string location, string reviewer, int sequence, string department, string salaryBand, int retries, double score, int graceDays, RegionalBalancePolicyStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Location { get; set; } = location;
    public string Reviewer { get; set; } = reviewer;
    public int Sequence { get; set; } = sequence;
    public string Department { get; set; } = department;
    public string SalaryBand { get; set; } = salaryBand;
    public int Retries { get; set; } = retries;
    public double Score { get; set; } = score;
    public int GraceDays { get; set; } = graceDays;
    public RegionalBalancePolicyStatus Status { get; set; } = status;

    /// <summary>Starts a regional balance policy in status planned.</summary>
    public static RegionalBalancePolicy NewRegionalBalancePolicy(string code, string title)
    {
        return new RegionalBalancePolicy(
            code,
            title,
            "",
            "",
            0,
            "",
            "",
            0,
            0.0,
            0,
            RegionalBalancePolicyStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RegionalBalancePolicyStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RegionalBalancePolicyStatus next)
    {
        switch (Status)
        {
            case RegionalBalancePolicyStatus.Planned:
                switch (next)
                {
                    case RegionalBalancePolicyStatus.Scheduled:
                        return true;
                }
                return false;
            case RegionalBalancePolicyStatus.Scheduled:
                switch (next)
                {
                    case RegionalBalancePolicyStatus.InProgress:
                    case RegionalBalancePolicyStatus.Cancelled:
                        return true;
                }
                return false;
            case RegionalBalancePolicyStatus.InProgress:
                switch (next)
                {
                    case RegionalBalancePolicyStatus.Completed:
                    case RegionalBalancePolicyStatus.Cancelled:
                        return true;
                }
                return false;
            case RegionalBalancePolicyStatus.Completed:
                switch (next)
                {
                    case RegionalBalancePolicyStatus.Cancelled:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(RegionalBalancePolicyStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"regional balance policy {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"regional balance policy"} {Code}";
    }

    public double WeightedTotal()
    {
        return Math.Round(Score - (double)GraceDays, 2);
    }

    public RegionalBalancePolicy Copy()
    {
        return new RegionalBalancePolicy(
            Code,
            Title,
            Location,
            Reviewer,
            Sequence,
            Department,
            SalaryBand,
            Retries,
            Score,
            GraceDays,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Department);
    }
}
