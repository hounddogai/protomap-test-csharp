namespace Acme.Workforce.Absence.Balance.BalanceGroup;

using Acme.Workforce.Kit.Text;

/// <summary>Where a capped balance quota is in its life cycle.</summary>
public enum CappedBalanceQuotaStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>The record of a capped balance quota and its current status.</summary>
public sealed class CappedBalanceQuota(string code, string label, double threshold, double hours, string channel, double cost, bool archived, string salaryBand, double budget, string notes, CappedBalanceQuotaStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public double Threshold { get; set; } = threshold;
    public double Hours { get; set; } = hours;
    public string Channel { get; set; } = channel;
    public double Cost { get; set; } = cost;
    public bool Archived { get; set; } = archived;
    public string SalaryBand { get; set; } = salaryBand;
    public double Budget { get; set; } = budget;
    public string Notes { get; set; } = notes;
    public CappedBalanceQuotaStatus Status { get; set; } = status;

    /// <summary>Starts a capped balance quota in status created.</summary>
    public static CappedBalanceQuota NewCappedBalanceQuota(string code, string label)
    {
        return new CappedBalanceQuota(
            code,
            label,
            0.0,
            0.0,
            "",
            0.0,
            false,
            "",
            0.0,
            "",
            CappedBalanceQuotaStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case CappedBalanceQuotaStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(CappedBalanceQuotaStatus next)
    {
        switch (Status)
        {
            case CappedBalanceQuotaStatus.Created:
                return next == CappedBalanceQuotaStatus.Validated || next == CappedBalanceQuotaStatus.Retired;
            case CappedBalanceQuotaStatus.Validated:
                return next == CappedBalanceQuotaStatus.Published || next == CappedBalanceQuotaStatus.Retired;
            case CappedBalanceQuotaStatus.Published:
                switch (next)
                {
                    case CappedBalanceQuotaStatus.Retired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(CappedBalanceQuotaStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"capped balance quota {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} [{Label}]";
    }

    public double ProjectedValue()
    {
        return Math.Round(Threshold * Budget, 2);
    }

    public CappedBalanceQuota Copy()
    {
        return new CappedBalanceQuota(
            Code,
            Label,
            Threshold,
            Hours,
            Channel,
            Cost,
            Archived,
            SalaryBand,
            Budget,
            Notes,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Channel);
    }
}
