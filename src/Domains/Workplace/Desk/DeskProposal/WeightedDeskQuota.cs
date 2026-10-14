namespace Acme.Workforce.Workplace.Desk.DeskProposal;

using Acme.Workforce.Kit.Text;

/// <summary>Where a weighted desk quota is in its life cycle.</summary>
public enum WeightedDeskQuotaStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>The record of a weighted desk quota and its current status.</summary>
public sealed class WeightedDeskQuota(string code, string label, string salaryBand, string title, string costCenter, bool pinned, double fte, Dictionary<string, string> attributes, string employeeId, int batchSize, WeightedDeskQuotaStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string SalaryBand { get; set; } = salaryBand;
    public string Title { get; set; } = title;
    public string CostCenter { get; set; } = costCenter;
    public bool Pinned { get; set; } = pinned;
    public double Fte { get; set; } = fte;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public string EmployeeId { get; set; } = employeeId;
    public int BatchSize { get; set; } = batchSize;
    public WeightedDeskQuotaStatus Status { get; set; } = status;

    /// <summary>Starts a weighted desk quota in status requested.</summary>
    public static WeightedDeskQuota NewWeightedDeskQuota(string code, string label)
    {
        return new WeightedDeskQuota(
            code,
            label,
            "",
            "",
            "",
            false,
            0.0,
            new Dictionary<string, string>(),
            "",
            0,
            WeightedDeskQuotaStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case WeightedDeskQuotaStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(WeightedDeskQuotaStatus next)
    {
        switch (Status)
        {
            case WeightedDeskQuotaStatus.Requested:
                return next == WeightedDeskQuotaStatus.Granted;
            case WeightedDeskQuotaStatus.Granted:
                return next == WeightedDeskQuotaStatus.Expired;
            case WeightedDeskQuotaStatus.Revoked:
                return next == WeightedDeskQuotaStatus.Expired;
        }
        return false;
    }

    public void MoveTo(WeightedDeskQuotaStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"weighted desk quota {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} / {Label}";
    }

    public string Attribute(string key, string fallback)
    {
        if (!Attributes.TryGetValue(key, out var found))
        {
            return fallback;
        }
        if (found.Trim() == "")
        {
            return fallback;
        }
        return found;
    }

    public double ProjectedVolume()
    {
        return Math.Round(Fte - (double)BatchSize, 2);
    }

    public WeightedDeskQuota Copy()
    {
        return new WeightedDeskQuota(
            Code,
            Label,
            SalaryBand,
            Title,
            CostCenter,
            Pinned,
            Fte,
            Attributes,
            EmployeeId,
            BatchSize,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
