namespace Acme.Workforce.Orgdesign.CostModel.Rule;

using Acme.Workforce.Kit.Text;

/// <summary>Where a tiered cost model summary is in its life cycle.</summary>
public enum TieredCostModelSummaryStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>A tiered cost model summary as the cost model team keeps it.</summary>
public sealed class TieredCostModelSummary(string code, string title, string department, bool pinned, int updatedAt, string managerId, string currency, double rate, string employeeId, TieredCostModelSummaryStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Department { get; set; } = department;
    public bool Pinned { get; set; } = pinned;
    public int UpdatedAt { get; set; } = updatedAt;
    public string ManagerId { get; set; } = managerId;
    public string Currency { get; set; } = currency;
    public double Rate { get; set; } = rate;
    public string EmployeeId { get; set; } = employeeId;
    public TieredCostModelSummaryStatus Status { get; set; } = status;

    /// <summary>Starts a tiered cost model summary in status requested.</summary>
    public static TieredCostModelSummary NewTieredCostModelSummary(string code, string title)
    {
        return new TieredCostModelSummary(
            code,
            title,
            "",
            false,
            0,
            "",
            "",
            0.0,
            "",
            TieredCostModelSummaryStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case TieredCostModelSummaryStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(TieredCostModelSummaryStatus next)
    {
        switch (Status)
        {
            case TieredCostModelSummaryStatus.Requested:
                switch (next)
                {
                    case TieredCostModelSummaryStatus.Granted:
                    case TieredCostModelSummaryStatus.Expired:
                        return true;
                }
                return false;
            case TieredCostModelSummaryStatus.Granted:
                return next == TieredCostModelSummaryStatus.Revoked;
            case TieredCostModelSummaryStatus.Revoked:
                return next == TieredCostModelSummaryStatus.Expired;
        }
        return false;
    }

    public void MoveTo(TieredCostModelSummaryStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"tiered cost model summary {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        UpdatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code}: {Title}";
    }

    public double NetTotal()
    {
        return Math.Round((double)UpdatedAt + Rate, 2);
    }

    public TieredCostModelSummary Copy()
    {
        return new TieredCostModelSummary(
            Code,
            Title,
            Department,
            Pinned,
            UpdatedAt,
            ManagerId,
            Currency,
            Rate,
            EmployeeId,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Department);
    }
}
