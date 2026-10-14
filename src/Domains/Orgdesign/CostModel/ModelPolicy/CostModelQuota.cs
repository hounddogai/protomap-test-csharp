namespace Acme.Workforce.Orgdesign.CostModel.ModelPolicy;

using Acme.Workforce.Kit.Text;

/// <summary>Where a cost model quota is in its life cycle.</summary>
public enum CostModelQuotaStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>The record of a cost model quota and its current status.</summary>
public sealed class CostModelQuota(string code, string name, double rating, int limit, string checksum, string region, string department, double budget, CostModelQuotaStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public double Rating { get; set; } = rating;
    public int Limit { get; set; } = limit;
    public string Checksum { get; set; } = checksum;
    public string Region { get; set; } = region;
    public string Department { get; set; } = department;
    public double Budget { get; set; } = budget;
    public CostModelQuotaStatus Status { get; set; } = status;

    /// <summary>Starts a cost model quota in status proposed.</summary>
    public static CostModelQuota NewCostModelQuota(string code, string name)
    {
        return new CostModelQuota(code, name, 0.0, 0, "", "", "", 0.0, CostModelQuotaStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case CostModelQuotaStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(CostModelQuotaStatus next)
    {
        switch (Status)
        {
            case CostModelQuotaStatus.Proposed:
                switch (next)
                {
                    case CostModelQuotaStatus.Approved:
                    case CostModelQuotaStatus.Rejected:
                        return true;
                }
                return false;
            case CostModelQuotaStatus.Approved:
                return next == CostModelQuotaStatus.Rejected;
            case CostModelQuotaStatus.Rejected:
                switch (next)
                {
                    case CostModelQuotaStatus.Withdrawn:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(CostModelQuotaStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"cost model quota {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"cost model quota"} {Code}";
    }

    public double NetValue()
    {
        return Math.Round((double)Limit + Budget, 2);
    }

    public CostModelQuota Copy()
    {
        return new CostModelQuota(Code, Name, Rating, Limit, Checksum, Region, Department, Budget, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Region);
    }
}
